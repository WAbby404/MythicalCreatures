namespace MythicalCreatures.Server.Models
{
    public class Region
    {
        public int Id { get; set; }
        public string RegionName { get; set; }
        public string Climate { get; set; }
        public List<CreatureXRegion> RegionWithCreature { get; set; }
    }
}
