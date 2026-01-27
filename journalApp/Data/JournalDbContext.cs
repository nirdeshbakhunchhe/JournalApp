using JournalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Data;

public class JournalDbContext : DbContext
{
    public JournalDbContext(DbContextOptions<JournalDbContext> options) : base(options) { }

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure "one journal entry per day"
        modelBuilder.Entity<JournalEntry>()
            .HasIndex(e => e.EntryDate)
            .IsUnique();

        // Many-to-many (EF Core will create join table automatically)
        modelBuilder.Entity<JournalEntry>()
            .HasMany(e => e.Tags)
            .WithMany(t => t.JournalEntries);

        base.OnModelCreating(modelBuilder);
    }
}
