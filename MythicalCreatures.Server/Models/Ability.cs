namespace MythicalCreatures.Server.Models
{
    public class Ability
    {
        public int Id { get; set; }
        public string AbilityName { get; set; }
        public Creature Creature { get; set; }
        public int CreatureId { get; set; }
    }
}

