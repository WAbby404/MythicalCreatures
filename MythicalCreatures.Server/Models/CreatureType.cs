namespace MythicalCreatures.Server.Models
{
    public class CreatureType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Creature> AllCreatures { get; set; }
    }
}