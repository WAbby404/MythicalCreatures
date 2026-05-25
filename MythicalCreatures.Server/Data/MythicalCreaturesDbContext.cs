using Microsoft.EntityFrameworkCore;
using MythicalCreatures.Server.Models;

namespace MythicalCreatures.Server.Data
{
    public class MythicalCreaturesDbContext(DbContextOptions<MythicalCreaturesDbContext> options) : DbContext(options)
    {
        public DbSet<Creature> Creatures { get; set; }
        public DbSet<CreatureType> CreatureTypes { get; set; }
        public DbSet<CreatureXRegion> CreatureXRegion { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Ability> Abilities { get; set; }

    }
}
