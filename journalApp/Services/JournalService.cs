using JournalApp.Data;
using JournalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalApp.Services;

public class JournalService
{
    private readonly IDbContextFactory<JournalDbContext> _factory;

    public JournalService(IDbContextFactory<JournalDbContext> factory)
    {
        _factory = factory;
    }

    // ----------------- CRUD Methods -----------------

    // Get journal entry by date
    public async Task<JournalEntry?> GetByDateAsync(DateOnly date)
    {
        using var db = _factory.CreateDbContext();
        return await db.JournalEntries
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.EntryDate == date);
    }

    // Get recent entries with pagination
    public async Task<List<JournalEntry>> GetRecentAsync(int page = 1, int pageSize = 10)
    {
        using var db = _factory.CreateDbContext();
        return await db.JournalEntries
            .OrderByDescending(e => e.EntryDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    // Create or update entry (one entry per day)
    public async Task<JournalEntry> UpsertAsync(JournalEntry entry)
    {
        using var db = _factory.CreateDbContext();
        var existing = await db.JournalEntries
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.EntryDate == entry.EntryDate);

        var now = DateTime.Now;

        if (existing is null)
        {
            entry.CreatedAt = now;
            entry.UpdatedAt = now;
            db.JournalEntries.Add(entry);
        }
        else
        {
            existing.Title = entry.Title;
            existing.Content = entry.Content;
            existing.PrimaryMood = entry.PrimaryMood;
            existing.SecondaryMood1 = entry.SecondaryMood1;
            existing.SecondaryMood2 = entry.SecondaryMood2;
            existing.Category = entry.Category;
            existing.UpdatedAt = now;
            // Tags updated separately
        }

        await db.SaveChangesAsync();
        return existing ?? entry;
    }

    // Delete entry by date
    public async Task DeleteByDateAsync(DateOnly date)
    {
        using var db = _factory.CreateDbContext();
        var existing = await db.JournalEntries.FirstOrDefaultAsync(e => e.EntryDate == date);
        if (existing is null) return;

        db.JournalEntries.Remove(existing);
        await db.SaveChangesAsync();
    }

    // Update tags for a journal entry
    public async Task UpdateTagsAsync(JournalEntry entry, List<string> tagNames)
    {
        using var db = _factory.CreateDbContext();
        var existingEntry = await db.JournalEntries
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.Id == entry.Id);

        if (existingEntry == null) return;

        existingEntry.Tags.Clear();

        foreach (var name in tagNames.Distinct())
        {
            var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name == name);
            if (tag == null)
            {
                tag = new Tag { Name = name };
                db.Tags.Add(tag);
            }
            existingEntry.Tags.Add(tag);
        }

        await db.SaveChangesAsync();
    }

    // ----------------- Analytics Methods -----------------

    // Mood distribution (primary + secondary moods)
    public async Task<Dictionary<string, int>> GetMoodDistributionAsync()
    {
        using var db = _factory.CreateDbContext();

        // Fetch entries first
        var entries = await db.JournalEntries
            .AsNoTracking()
            .ToListAsync();

        var allMoods = entries
            .SelectMany(e => new[] { e.PrimaryMood, e.SecondaryMood1, e.SecondaryMood2 })
            .Where(m => !string.IsNullOrEmpty(m))
            .ToList();

        return allMoods
            .GroupBy(m => m!)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Tag usage count
    public async Task<Dictionary<string, int>> GetTagUsageAsync()
    {
        using var db = _factory.CreateDbContext();

        var entries = await db.JournalEntries
            .Include(e => e.Tags)
            .AsNoTracking()
            .ToListAsync();

        var allTags = entries
            .SelectMany(e => e.Tags.Select(t => t.Name))
            .ToList();

        return allTags
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Word count per entry date
    public async Task<Dictionary<DateOnly, int>> GetWordCountTrendsAsync()
    {
        using var db = _factory.CreateDbContext();

        var entries = await db.JournalEntries.AsNoTracking().ToListAsync();

        return entries.ToDictionary(
            e => e.EntryDate,
            e => string.IsNullOrWhiteSpace(e.Content)
                ? 0
                : e.Content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length
        );
    }
}
