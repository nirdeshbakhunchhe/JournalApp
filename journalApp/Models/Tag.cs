using System.ComponentModel.DataAnnotations;

namespace JournalApp.Models;

public class Tag
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public List<JournalEntry> JournalEntries { get; set; } = new();
}
