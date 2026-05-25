namespace MythicalCreatures.Server.DTOs
{
    public class UpdateCreatureDto
    {
        //public int Id { get; set; } -will get this from the route!
        public string? Name { get; set; }
        public int? CreatureTypeId { get; set; }
        public double? PowerLevel { get; set; }
        public bool? IsDangerous { get; set; }
        public DateTime? DateFirstSighted { get; set; }
        public List<string>? Abilities { get; set; }
        public List<int>? FoundInRegions { get; set; }
    }
}
