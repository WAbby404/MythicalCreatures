using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MythicalCreatures.Server.Data;
using MythicalCreatures.Server.DTOs;
using MythicalCreatures.Server.Models;


namespace MythicalCreatures.Server.Controllers
{
    [ApiController] // marks this as an API controller
    [Route("api/[controller]")] //sets the base URL to /api/creatures - creature comes from the class name; CreaturesController, it will know to call it whatever comes before 'controller'!
   
    public class CreaturesController : ControllerBase {
        private readonly MythicalCreaturesDbContext _context; // - stores the injected DbContext

        //this is the constructor! DI in action, we dont create the context ourselves
        public CreaturesController(MythicalCreaturesDbContext context)
        {
            _context = context; // _ prefix is a C# convention for a private field, you'll see this everywhere in real projects;
        }

        //GET /api/creatures
        //returns all creatures as a list of CreatureResponseDto
        [HttpGet]
        public ActionResult<List<CreatureResponseDto>> GetCreatures() { //ActionResult gives us access to return OK(result), StatusCode(500, ex.Message), NotFound(), BadRequest()
            try {
                //Queries the Creatures table and eagerly load related data
                //.Include() tells EF to load related CreatureType and Abilities
                //.ThenInclude() goes one level deeper - loads Region from inside CreatureXRegion
                var creatures = _context.Creatures
                    .Include(c => c.CreatureType)
                    .Include(c => c.Abilities)
                    .Include(c => c.FoundInRegions)
                        .ThenInclude(cr => cr.Region)
                    .ToList();

                //Map each Creature EF model to a CreatureResponseDto
                //This is important - we never return raw EF models directly from an API!
                //.Select() projects each creature into a new DTO shape
                var result = creatures.Select(c => new CreatureResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatureTypeName = c.CreatureType.Name, //flattens CreatureType to just its name
                    PowerLevel = c.PowerLevel,
                    IsDangerous = c.IsDangerous,
                    DateFirstSighted = c.DateFirstSighted,
                    Abilities = c.Abilities.Select(a => a.AbilityName).ToList(), //project to List<string>
                    FoundInRegions = c.FoundInRegions.Select(a => a.Region.RegionName).ToList() //project to List<string>
                });

                // return 200 OK with the results
                return Ok(result);
            } catch(Exception ex)
            {
                // return 500 Internal Server Error with the error message
                return StatusCode(500, ex.Message);
            }
        }

        //GET return one creature
        [HttpGet("{id}")]
        public ActionResult<CreatureResponseDto> GetCreature(int id) //takes in an id as a parameter from the URL route
        {
            try
            {
                var creature = _context.Creatures
                    .Include(c => c.CreatureType)
                    .Include(c => c.Abilities)
                    .Include(c => c.FoundInRegions)
                        .ThenInclude(cr => cr.Region)
                     .FirstOrDefault(c => c.Id == id); //.FirstOrDefault fetches one record instead of all - filters in the DATABASE! 
                //this is good so we dont grab all values and then filter that, we use SQL to only get what we need

                if (creature == null) return NotFound(); //this handles the case of no creature found, returns a 404 error

                var result = new CreatureResponseDto //mapping a single creature to a single ResponseDto - .Select() is not needed since this is only one creature
                {
                    Id = creature.Id,
                    Name = creature.Name,
                    CreatureTypeName = creature.CreatureType.Name,
                    PowerLevel = creature.PowerLevel,
                    IsDangerous = creature.IsDangerous,
                    DateFirstSighted = creature.DateFirstSighted,
                    Abilities = creature.Abilities.Select(a => a.AbilityName).ToList(),
                    FoundInRegions = creature.FoundInRegions.Select(a => a.Region.RegionName).ToList()
                };
                return Ok(result);

            } catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        //POST api/creatures
        [HttpPost]
        public ActionResult<CreatureResponseDto> CreateCreature([FromBody] CreateCreatureDto dto) //accepts a [FromBody] DTO instead of a URL param
        {
            try
            {
                //Fetch existing Region records from DB using IDs from params where matching 
                var regions = _context.Regions.Where(r => dto.FoundInRegions.Contains(r.Id)).ToList();

                //Create a new Creature EF model from the DTO (never save DTOs directly, always map to EF model)
                Creature creature = new Creature
                {
                    Name = dto.Name,
                    PowerLevel = dto.PowerLevel,
                    IsDangerous = dto.IsDangerous,
                    DateFirstSighted = dto.DateFirstSighted,
                    CreatureTypeId = dto.CreatureTypeId,
                    //Abilities came in as a List<string> (names) in the DTO
                    //We project each name into a new Ability EF object (EF will automatically set the CreatureId foreign key when saving)
                    Abilities = dto.Abilities.Select(a => new Ability { AbilityName = a }).ToList(),
                    //Regions come in as a List<int> in the DTO
                    //We already got the regions from the fetch above, now we wrap each Region in a CreatureXRegion join table object
                    FoundInRegions = regions.Select(r => new CreatureXRegion { Region = r }).ToList()
                };

                //Add the creature to the DbContext (this marks it as a pending insert)
                _context.Creatures.Add(creature);

                //Executes the INSERT statement and commits it to the DB
                _context.SaveChanges();

                //Re-fetch the saved creature with all related data loaded
                //We cant use the creature obj directly because navigation properties (like CreatureType are not loaded in memory after just saving)
                var saved = _context.Creatures
                    .Include(c => c.CreatureType)
                    .Include(c => c.Abilities)
                    .Include(c => c.FoundInRegions)
                        .ThenInclude(cr => cr.Region)
                    .FirstOrDefault(c => c.Id == creature.Id);

                //Map the fully loaded EF model to a CreatureResponseDto
                //Same mapping pattern as the GET endpoints
                var result = new CreatureResponseDto
                {
                    Id = saved.Id,
                    Name = saved.Name,
                    CreatureTypeName = saved.CreatureType.Name,
                    PowerLevel = saved.PowerLevel,
                    IsDangerous = saved.IsDangerous,
                    DateFirstSighted = saved.DateFirstSighted,
                    Abilities = saved.Abilities.Select(a => a.AbilityName).ToList(),
                    FoundInRegions = saved.FoundInRegions.Select(a => a.Region.RegionName).ToList()
                };

                //Returns a 201 Created with:
                // - a location header pointing to GET /api/creatures/{id} for the new resource
                // - The created creature as a CreatureResponseDto in the response body
                return CreatedAtAction(nameof(GetCreature), new { id = creature.Id }, result);

            } catch(Exception ex){
                // Return 500 Internal Server Error with error message
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<CreatureResponseDto> UpdateCreature(int id, [FromBody] UpdateCreatureDto dto)
        {
            try {
                //getting the creature from the DB
                var creature = _context.Creatures
                    .Include(c => c.CreatureType)
                    .Include(c => c.Abilities)
                    .Include(c => c.FoundInRegions)
                        .ThenInclude(cr => cr.Region)
                    .FirstOrDefault(c => c.Id == id);

                //return if no dino was found
                if (creature == null) return NotFound();

                //conditional mapping, if the value exsists in the dto, update it in our found creature from DB
                if (dto.Name != null) creature.Name = dto.Name;
                if (dto.CreatureTypeId != null) creature.CreatureTypeId = (int)dto.CreatureTypeId;
                if (dto.PowerLevel != null) creature.PowerLevel = (double)dto.PowerLevel;
                if (dto.IsDangerous != null) creature.IsDangerous = (bool)dto.IsDangerous;
                if (dto.DateFirstSighted != null) creature.DateFirstSighted = (DateTime)dto.DateFirstSighted;
                if (dto.Abilities != null) creature.Abilities = dto.Abilities.Select(a => new Ability { AbilityName = a }).ToList();
                if (dto.FoundInRegions != null) {
                    var regions = _context.Regions.Where(r => dto.FoundInRegions.Contains(r.Id)).ToList();
                    creature.FoundInRegions = regions.Select(a => new CreatureXRegion { Region = a }).ToList();
                }
               
                //save changes after updated
                _context.SaveChanges();

                //regrab the creature from the DB
                var saved = _context.Creatures
                    .Include(c => c.CreatureType)
                    .Include(c => c.Abilities)
                    .Include(c => c.FoundInRegions)
                        .ThenInclude(cr => cr.Region)
                    .FirstOrDefault(c => c.Id == creature.Id);

                //Remap the values from the DB to a creature response dto
                var result = new CreatureResponseDto
                {
                    Id = saved.Id,
                    Name = saved.Name,
                    CreatureTypeName = saved.CreatureType.Name,
                    PowerLevel = saved.PowerLevel,
                    IsDangerous = saved.IsDangerous,
                    DateFirstSighted = saved.DateFirstSighted,
                    Abilities = saved.Abilities.Select(a => a.AbilityName).ToList(),
                    FoundInRegions = saved.FoundInRegions.Select(a => a.Region.RegionName).ToList()
                };

                //return that same creature
                return Ok(result);

            } catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //DELETE
        [HttpDelete("{id}")]
        public ActionResult DeleteCreature (int id)
        {
            //getting the creature from the DB
            try
            {
                var creature = _context.Creatures.FirstOrDefault(c => c.Id == id);

                if (creature == null) return NotFound();

                _context.Creatures.Remove(creature);

                _context.SaveChanges();

                return NoContent();
            } catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
