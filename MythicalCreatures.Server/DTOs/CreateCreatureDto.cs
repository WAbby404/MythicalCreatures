namespace MythicalCreatures.Server.DTOs
{
    public class CreateCreatureDto
    {
        public string Name { get; set; }
        public int CreatureTypeId { get; set; }
        public double PowerLevel { get; set; }
        public bool IsDangerous { get; set; }
        public DateTime DateFirstSighted { get; set; }
        public List<string> Abilities { get; set; }
        public List<int> FoundInRegions { get; set; }
    }
}
