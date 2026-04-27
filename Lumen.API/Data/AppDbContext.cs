using Lumen.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lumen.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>          Users          => Set<User>();
    public DbSet<Character>     Characters     => Set<Character>();
    public DbSet<Connection>    Connections    => Set<Connection>();
    public DbSet<DailyRecord>   DailyRecords   => Set<DailyRecord>();
    public DbSet<MoodLog>       MoodLogs       => Set<MoodLog>();
    public DbSet<FoodLog>       FoodLogs       => Set<FoodLog>();
    public DbSet<SleepLog>      SleepLogs      => Set<SleepLog>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ── User ──────────────────────────────────────────────────────────────
        b.Entity<User>(u =>
        {
            u.HasIndex(x => x.Email).IsUnique();
            u.HasIndex(x => x.Login).IsUnique();
            u.HasIndex(x => x.InviteCode).IsUnique();

            u.HasOne(x => x.Character)
             .WithMany(c => c.Users)
             .HasForeignKey(x => x.CharacterId)
             .OnDelete(DeleteBehavior.Restrict);

            u.HasOne(x => x.Connection)
             .WithMany(c => c.Users)
             .HasForeignKey(x => x.ConnectionId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── DailyRecord ───────────────────────────────────────────────────────
        b.Entity<DailyRecord>(d =>
        {
            d.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
        });

        // ── SleepLog (1:1 з DailyRecord) ─────────────────────────────────────
        b.Entity<SleepLog>()
         .HasOne(s => s.DailyRecord)
         .WithOne(d => d.SleepLog)
         .HasForeignKey<SleepLog>(s => s.DailyRecordId);

        // ── MoodLog: один TimeSlot на день ───────────────────────────────────
        b.Entity<MoodLog>()
         .HasIndex(m => new { m.DailyRecordId, m.TimeSlot })
         .IsUnique();

        // ── FoodLog: один MealType на день ───────────────────────────────────
        b.Entity<FoodLog>()
         .HasIndex(f => new { f.DailyRecordId, f.MealType })
         .IsUnique();

        // ── Seed characters ───────────────────────────────────────────────────
        b.Entity<Character>().HasData(
            new Character { Id = 1, Name = "Panda", AssetKey = "panda", Description = "Calm and cozy panda" },
            new Character { Id = 2, Name = "Pig",   AssetKey = "pig",   Description = "Cheerful little pig"  }
        );
    }
}
