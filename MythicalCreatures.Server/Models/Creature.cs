namespace MythicalCreatures.Server.Models
{
    public class Creature
    {
        public int Id { get; set; }
        public int CreatureTypeId { get; set; }
        public CreatureType CreatureType { get; set; }
        public string Name { get; set; }
        public double PowerLevel { get; set; }
        public bool IsDangerous { get; set; }
        public DateTime DateFirstSighted { get; set; }
        public List<Ability> Abilities { get; set; }
        public List<CreatureXRegion> FoundInRegions { get; set; }
    }
}
