using System.Security.Cryptography;
using System.Text;
namespace JournalApp.Services;

public class AuthService
{
    private const string PIN_KEY = "journal_pin_hash";
    private bool _isAuthenticated = false;
    public AuthService()
    {
        // IMPORTANT: Always start as NOT authenticated on app launch
        _isAuthenticated = false;
        System.Diagnostics.Debug.WriteLine("AuthService CONSTRUCTOR - IsAuthenticated: false");
    }
    public bool IsAuthenticated
    {
        get
        {
            System.Diagnostics.Debug.WriteLine($"IsAuthenticated called - returning: {_isAuthenticated}");
            return _isAuthenticated;
        }
    }
    // Check if PIN is set up
    public bool IsPinSetup()
    {
        return Preferences.ContainsKey(PIN_KEY);
    }
    // Set up PIN for first time
    public void SetupPin(string pin)
    {
        var hash = HashPin(pin);
        Preferences.Set(PIN_KEY, hash);
        _isAuthenticated = true; // Auto-login after setup
        System.Diagnostics.Debug.WriteLine("SetupPin called - IsAuthenticated: true");
    }
    // Verify PIN
    public bool VerifyPin(string pin)
    {
        if (!IsPinSetup())
            return false;
        var storedHash = Preferences.Get(PIN_KEY, string.Empty);
        var inputHash = HashPin(pin);
        if (storedHash == inputHash)
        {
            _isAuthenticated = true;
            System.Diagnostics.Debug.WriteLine("VerifyPin SUCCESS - IsAuthenticated: true");
            return true;
        }
        System.Diagnostics.Debug.WriteLine("VerifyPin FAILED");
        return false;
    }
    // Change PIN
    public bool ChangePin(string oldPin, string newPin)
    {
        if (!VerifyPin(oldPin))
            return false;
        SetupPin(newPin);
        return true;
    }
    // Lock the app
    public void Lock()
    {
        _isAuthenticated = false;
        System.Diagnostics.Debug.WriteLine("Lock called - IsAuthenticated: false");
    }
    // Hash the PIN using SHA256
    private string HashPin(string pin)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(pin);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}