using Microsoft.Maui.Controls;
using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models;

public class JournalEntry
{
    public int Id { get; set; }

    // One entry per day -> DateOnly is perfect for that rule
    public DateOnly EntryDate { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    // Markdown content
    public string Content { get; set; } = string.Empty;

    // Mood (simple for now; we’ll refine later)
    [Required, MaxLength(30)]
    public string PrimaryMood { get; set; } = "Neutral";

    [MaxLength(30)]
    public string? SecondaryMood1 { get; set; }

    [MaxLength(30)]
    public string? SecondaryMood2 { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    // System timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Tags (many-to-many later; for now, quick join table)
    public List<Tag> Tags { get; set; } = new();
}
