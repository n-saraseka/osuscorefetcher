using osuscorefetcher.ConfigHandler;
using osuscorefetcher.ApiClasses;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
namespace osuscorefetcher.DbService;

public class ScoreFetcherContext : DbContext
{
    private static readonly Config Config = ConfigIO.GetConfig();
    private static readonly string ConnectionString = $"Host={Config.DbHost};Username={Config.DbUsername};Password={Config.DbPassword};Database={Config.DbName}";
    public DbSet<APIBeatmap> Beatmaps { get; set; }
    public DbSet<Beatmapset> Beatmapsets { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<User> Users { get; set; }

    public ScoreFetcherContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql(ConnectionString,
            o => o
                .MapEnum<Mode>("mode")
                .MapEnum<Grade>("grade")
                .MapEnum<BeatmapStatus>("beatmap_status"))
            .UseSnakeCaseNamingConvention();
}
