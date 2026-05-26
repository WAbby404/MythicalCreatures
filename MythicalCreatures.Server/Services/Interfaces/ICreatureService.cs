using MythicalCreatures.Server.DTOs;

namespace MythicalCreatures.Server.Services.Interfaces
{
    public interface ICreatureService
    {
        public List<CreatureResponseDto> GetCreatures();
        public CreatureResponseDto GetCreature(int id);
        public CreatureResponseDto CreateCreature( CreateCreatureDto dto);
        public CreatureResponseDto UpdateCreature(int id, UpdateCreatureDto dto);
        public void DeleteCreature(int id);
    }
}
