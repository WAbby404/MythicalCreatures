using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MythicalCreatures.Server.Data;
using MythicalCreatures.Server.DTOs;
using MythicalCreatures.Server.Models;
using MythicalCreatures.Server.Services.Interfaces;

namespace MythicalCreatures.Server.Services.Implementations
{
    public class CreatureService : ICreatureService
    {
        private readonly MythicalCreaturesDbContext _context; // - stores the injected DbContext
        private readonly ILogger<CreatureService> _logger;

        //this is the constructor! DI in action, we dont create the context ourselves
        public CreatureService(MythicalCreaturesDbContext context, ILogger<CreatureService> logger)
        {
            _context = context; // _ prefix is a C# convention for a private field, you'll see this everywhere in real projects;
            _logger = logger;
        }

        public List<CreatureResponseDto> GetCreatures()
        {
            try
            {
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
                }).ToList();

                _logger.LogInformation("Successfully fetched all creatures");
                return result;
            }
            catch (Exception ex)
            {
                //Throws an exception with a descriptive error message
                _logger.LogError(ex, "Error fetching all creatures");
                throw new Exception($"Error fetching creatures: {ex.Message}", ex);
            }
        }

        public CreatureResponseDto GetCreature(int id)
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

                if (creature == null) throw new KeyNotFoundException($"Creature with ID {id} not found."); //this handles the case of no creature found, returns a 404 error


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
                _logger.LogInformation("Successfully fetched creature with ID {id}", id);
                return result;
                throw new Exception($"Error fetching creature: {ex.Message}", ex);
            }
        }

        public CreatureResponseDto CreateCreature(CreateCreatureDto dto)
        {
            try
            {
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

                _logger.LogInformation("Successfully created creature with ID {id}", result.Id);
                return result;
                
            } catch (Exception ex)
            {
                throw new Exception($"Error creating a new Creature: {ex.Message}", ex);
            }
        }

        public CreatureResponseDto UpdateCreature(int id, UpdateCreatureDto dto)
        {
            try
            {
                //getting the creature from the DB
                var creature = _context.Creatures
                    .Include(c => c.CreatureType)
                    .Include(c => c.Abilities)
                    .Include(c => c.FoundInRegions)
                        .ThenInclude(cr => cr.Region)
                    .FirstOrDefault(c => c.Id == id);

                //return if no dino was found
                if (creature == null) throw new KeyNotFoundException($"Creature with ID {id} not found.");

                //conditional mapping, if the value exsists in the dto, update it in our found creature from DB
                if (dto.Name != null) creature.Name = dto.Name;
                if (dto.CreatureTypeId != null) creature.CreatureTypeId = (int)dto.CreatureTypeId;
                if (dto.PowerLevel != null) creature.PowerLevel = (double)dto.PowerLevel;
                if (dto.IsDangerous != null) creature.IsDangerous = (bool)dto.IsDangerous;
                if (dto.DateFirstSighted != null) creature.DateFirstSighted = (DateTime)dto.DateFirstSighted;
                if (dto.Abilities != null) creature.Abilities = dto.Abilities.Select(a => new Ability { AbilityName = a }).ToList();
                if (dto.FoundInRegions != null)
                {
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

                _logger.LogInformation("Successfully updated creature with ID {id}", id);
                return result;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating creature");
                throw new Exception($"Error updating creature {id}: {ex.Message}", ex);
            }
        }

        public void DeleteCreature(int id)
        {
            try
            {
                var creature = _context.Creatures.FirstOrDefault(c => c.Id == id);

                if (creature == null) throw new KeyNotFoundException($"Creature with ID {id} not found.");

                _context.Creatures.Remove(creature);

                _context.SaveChanges();

                _logger.LogInformation("Successfully deleted creature with ID {id}", id);
            } catch(Exception ex)
            {
                throw new Exception($"Error deleting creature {id}: {ex.Message}", ex);
            }
        }
    }
}
