

using JournalApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace JournalApp.Services;

public class PdfExportService
{
    private readonly JournalService _journalService;

    public PdfExportService(JournalService journalService)
    {
        _journalService = journalService;

        // Set license (Community license is free)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> ExportToPdfAsync(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            // Get all entries in date range
            var allEntries = new List<JournalEntry>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var entry = await _journalService.GetByDateAsync(currentDate);
                if (entry != null)
                {
                    allEntries.Add(entry);
                }
                currentDate = currentDate.AddDays(1);
            }

            if (allEntries.Count == 0)
            {
                return "error:No entries found in the selected date range.";
            }

            // Generate PDF
            var fileName = $"Journal_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            var outputPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            // Run PDF generation in background task
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(50);
                        page.PageColor("#FFFFFF");
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(content => ComposeContent(content, allEntries, startDate, endDate));
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                }).GeneratePdf(outputPath);
            });

            return $"success:{outputPath}";
        }
        catch (Exception ex)
        {
            return $"error:Error exporting PDF: {ex.Message}";
        }
    }

    private void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Journal Export").FontSize(20).SemiBold().FontColor("#1976D2");
                column.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy}").FontSize(9).FontColor("#666666");
            });
        });
    }

    private void ComposeContent(QuestPDF.Infrastructure.IContainer container, List<JournalEntry> entries, DateOnly startDate, DateOnly endDate)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(10);

            // Summary
            column.Item().Background("#F5F5F5").Padding(15).Column(summary =>
            {
                summary.Item().Text($"Period: {startDate:MMMM dd, yyyy} - {endDate:MMMM dd, yyyy}").FontSize(12);
                summary.Item().Text($"Total Entries: {entries.Count}").FontSize(12);
            });

            column.Item().PaddingVertical(15).LineHorizontal(1).LineColor("#CCCCCC");

            // Entries
            foreach (var entry in entries.OrderBy(e => e.EntryDate))
            {
                column.Item().Element(c => ComposeEntry(c, entry));
                column.Item().PageBreak(); // Each entry on new page
            }
        });
    }

    private void ComposeEntry(QuestPDF.Infrastructure.IContainer container, JournalEntry entry)
    {
        container.Border(1).BorderColor("#E0E0E0").Padding(20).Column(column =>
        {
            column.Spacing(10);

            // Title and Date
            column.Item().Background("#F5F5F5").Padding(10).Column(header =>
            {
                header.Item().Text(entry.Title).FontSize(18).SemiBold();
                header.Item().Text(entry.EntryDate.ToString("dddd, MMMM dd, yyyy")).FontSize(10).Italic().FontColor("#666666");
            });

            // Moods
            column.Item().Row(row =>
            {
                row.AutoItem().Text("Moods: ").SemiBold();
                row.AutoItem().PaddingLeft(5).Text(entry.PrimaryMood).FontSize(10).FontColor("#1976D2");

                if (!string.IsNullOrEmpty(entry.SecondaryMood1))
                {
                    row.AutoItem().PaddingLeft(5).Text($", {entry.SecondaryMood1}").FontSize(10).FontColor("#1976D2");
                }

                if (!string.IsNullOrEmpty(entry.SecondaryMood2))
                {
                    row.AutoItem().PaddingLeft(5).Text($", {entry.SecondaryMood2}").FontSize(10).FontColor("#1976D2");
                }
            });

            // Category
            if (!string.IsNullOrEmpty(entry.Category))
            {
                column.Item().Row(row =>
                {
                    row.AutoItem().Text("Category: ").SemiBold();
                    row.AutoItem().PaddingLeft(5).Text(entry.Category);
                });
            }

            // Tags
            if (entry.Tags != null && entry.Tags.Count > 0)
            {
                column.Item().Row(row =>
                {
                    row.AutoItem().Text("Tags: ").SemiBold();
                    row.AutoItem().PaddingLeft(5).Text(string.Join(", ", entry.Tags.Select(t => t.Name))).FontColor("#E65100");
                });
            }

            // Separator
            column.Item().PaddingVertical(10).LineHorizontal(0.5f).LineColor("#EEEEEE");

            // Content
            column.Item().Text(entry.Content ?? "").FontSize(10).LineHeight(1.5f);

            // Metadata footer
            column.Item().PaddingTop(15).Row(row =>
            {
                row.AutoItem().Text($"Created: {entry.CreatedAt:g}").FontSize(8).FontColor("#999999");
                if (entry.UpdatedAt != entry.CreatedAt)
                {
                    row.AutoItem().PaddingLeft(15).Text($"Updated: {entry.UpdatedAt:g}").FontSize(8).FontColor("#999999");
                }
            });
        });
    }
}