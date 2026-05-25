namespace MythicalCreatures.Server.DTOs
{
    public class CreatureResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatureTypeName { get; set; }
        public double PowerLevel { get; set; }
        public bool IsDangerous { get; set; }
        public DateTime DateFirstSighted { get; set; }
        public List<string> Abilities { get; set; }
        public List<string> FoundInRegions { get; set; }
    }
}
