using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using osu.Game.Online.API;
using osuscorefetcher.ApiClasses;
using osuscorefetcher.ConfigHandler;
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

public class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.Property(s => s.Statistics).HasConversion(
                                            v => JsonConvert.SerializeObject(v),
                                            v => JsonConvert.DeserializeObject<Statistics>(v,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        builder.Property(s => s.MaximumStatistics).HasConversion(
                                            v => JsonConvert.SerializeObject(v),
                                            v => JsonConvert.DeserializeObject<Statistics>(v,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        builder.Property(s => s.Mods).HasConversion(
                                            v => JsonConvert.SerializeObject(v),
                                            v => JsonConvert.DeserializeObject<APIMod[]>(v,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .OwnsOne(u => u.Country, country =>
            {
                country.ToTable("UserCountry");
                country.WithOwner().HasForeignKey("user_id");
            });
        builder.Property(s => s.RulesetStatistics).HasConversion(
                                            v => JsonConvert.SerializeObject(v),
                                            v => JsonConvert.DeserializeObject<Dictionary<string, UserRulesetStatistics>>(v,
                                            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
    }
}

public class BeatmapConfiguration : IEntityTypeConfiguration<APIBeatmap>
{
    public void Configure(EntityTypeBuilder<APIBeatmap> builder)
    {
        builder
            .HasOne(b => b.Beatmapset)
            .WithMany(bs => bs.Beatmaps)
            .HasForeignKey(b => b.BeatmapsetId)
            .HasPrincipalKey(bs => bs.Id);
    }
}
    