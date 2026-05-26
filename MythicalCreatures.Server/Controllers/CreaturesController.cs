using Microsoft.AspNetCore.Mvc;
using MythicalCreatures.Server.DTOs;
using MythicalCreatures.Server.Services.Interfaces;


namespace MythicalCreatures.Server.Controllers
{
    [ApiController] // marks this as an API controller
    [Route("api/[controller]")] //sets the base URL to /api/creatures - creature comes from the class name; CreaturesController, it will know to call it whatever comes before 'controller'!
   
    public class CreaturesController : ControllerBase {
        private readonly ICreatureService _creatureService; // - stores the injected DbContext

        //this is the constructor! DI in action, we dont create the context ourselves
        public CreaturesController(ICreatureService creatureService)
        {
            _creatureService = creatureService; // _ prefix is a C# convention for a private field, you'll see this everywhere in real projects;
        }

        //GET /api/creatures
        //returns all creatures as a list of CreatureResponseDto
        [HttpGet]
        public ActionResult<List<CreatureResponseDto>> GetCreatures() { //ActionResult gives us access to return OK(result), StatusCode(500, ex.Message), NotFound(), BadRequest()
            //moved to a service layer!
            try {
                var result = _creatureService.GetCreatures();
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
                var result = _creatureService.GetCreature(id);

                //if KeyNotFoundException return NotFound();
                return Ok(result);
            } catch (KeyNotFoundException)
            {
                return NotFound();
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
                var result = _creatureService.CreateCreature(dto);

                //Returns a 201 Created with:
                // - a location header pointing to GET /api/creatures/{id} for the new resource
                // - The created creature as a CreatureResponseDto in the response body
                return CreatedAtAction(nameof(GetCreature), new { id = result.Id }, result);

            } catch(Exception ex){
                // Return 500 Internal Server Error with error message
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<CreatureResponseDto> UpdateCreature(int id, [FromBody] UpdateCreatureDto dto)
        {
            try {
                var result = _creatureService.UpdateCreature(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch(Exception ex)
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
                _creatureService.DeleteCreature(id);
                return NoContent();
            } catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
