using JournalApp;
using JournalApp.Data;
using JournalApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JournalApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ✅ SQLite DB path
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");

        // ✅ Register EF Core DbContext
        builder.Services.AddDbContextFactory<JournalDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // ✅ Register service layer
        builder.Services.AddSingleton<JournalService>();

        // ✅ Build the app FIRST
        var app = builder.Build();

        // ✅ Create DB + tables after app is built
        using (var scope = app.Services.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<JournalDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.EnsureCreated();
        }

        return app;
    }
}
