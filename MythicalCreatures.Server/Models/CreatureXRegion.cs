namespace MythicalCreatures.Server.Models
{
    public class CreatureXRegion
    {
        public int Id { get; set; }
        public int CreatureId { get; set; }
        public Creature Creature { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
    }
}