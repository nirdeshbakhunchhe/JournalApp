using Microsoft.Maui.Storage;

namespace JournalApp.Services;

public class ThemeService
{
    private const string ThemeStorageKey = "app_theme";
    
    public string CurrentTheme { get; private set; } = "light-default";
    public event Action? OnThemeChanged;

    public static List<ThemeInfo> AvailableThemes => new()
    {
        new ThemeInfo { Id = "light-default", Name = "Light Default", Mode = "light", PrimaryColor = "#1b6ec2", SecondaryColor = "#2196f3", BgColor = "#ffffff" },
        new ThemeInfo { Id = "light-blue", Name = "Light Blue", Mode = "light", PrimaryColor = "#1976d2", SecondaryColor = "#42a5f5", BgColor = "#f8fbfd" },
        new ThemeInfo { Id = "light-purple", Name = "Light Purple", Mode = "light", PrimaryColor = "#7b1fa2", SecondaryColor = "#9c27b0", BgColor = "#faf8fc" },
        new ThemeInfo { Id = "dark-default", Name = "Dark Default", Mode = "dark", PrimaryColor = "#2196f3", SecondaryColor = "#42a5f5", BgColor = "#1e1e1e" },
        new ThemeInfo { Id = "dark-blue", Name = "Dark Blue", Mode = "dark", PrimaryColor = "#42a5f5", SecondaryColor = "#64b5f6", BgColor = "#0d1b2a" },
        new ThemeInfo { Id = "dark-purple", Name = "Dark Purple", Mode = "dark", PrimaryColor = "#ce93d8", SecondaryColor = "#e1bee7", BgColor = "#1a0d2e" },
        new ThemeInfo { Id = "dark-ocean", Name = "Ocean", Mode = "dark", PrimaryColor = "#00bcd4", SecondaryColor = "#4dd0e1", BgColor = "#0a1628" },
        new ThemeInfo { Id = "dark-forest", Name = "Forest", Mode = "dark", PrimaryColor = "#4caf50", SecondaryColor = "#81c784", BgColor = "#0f1f15" },
        new ThemeInfo { Id = "dark-sunset", Name = "Sunset", Mode = "dark", PrimaryColor = "#ff6f00", SecondaryColor = "#ffa040", BgColor = "#2a1810" },
    };

    public async Task InitializeAsync()
    {
        var savedTheme = await SecureStorage.GetAsync(ThemeStorageKey);
        if (!string.IsNullOrEmpty(savedTheme))
        {
            CurrentTheme = savedTheme;
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        if (AvailableThemes.Any(t => t.Id == theme))
        {
            CurrentTheme = theme;
            await SecureStorage.SetAsync(ThemeStorageKey, theme);
            OnThemeChanged?.Invoke();
        }
    }

    public ThemeInfo? GetCurrentThemeInfo() => AvailableThemes.FirstOrDefault(t => t.Id == CurrentTheme);
}

public class ThemeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string BgColor { get; set; } = string.Empty;
}
