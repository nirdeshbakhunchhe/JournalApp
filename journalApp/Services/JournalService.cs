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

    public async Task<JournalEntry?> GetByDateAsync(DateOnly date)
    {
        using var db = _factory.CreateDbContext();
        return await db.JournalEntries
            .Include(e => e.Tags)
            .FirstOrDefaultAsync(e => e.EntryDate == date);
    }

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

    // Create or Update (one entry per day rule enforced by unique index)
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
            await db.SaveChangesAsync();
            return entry;
        }

        existing.Title = entry.Title;
        existing.Content = entry.Content;
        existing.PrimaryMood = entry.PrimaryMood;
        existing.SecondaryMood1 = entry.SecondaryMood1;
        existing.SecondaryMood2 = entry.SecondaryMood2;
        existing.Category = entry.Category;
        existing.UpdatedAt = now;

        // Tags update later (keep simple for now)
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteByDateAsync(DateOnly date)
    {
        using var db = _factory.CreateDbContext();
        var existing = await db.JournalEntries.FirstOrDefaultAsync(e => e.EntryDate == date);
        if (existing is null) return;

        db.JournalEntries.Remove(existing);
        await db.SaveChangesAsync();
    }

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

}
