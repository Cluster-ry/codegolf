using System.Security.Cryptography;
using System.Text;

namespace CodeGolf.Services;

public sealed class AdminAccess
{
    private readonly string? _password;

    public AdminAccess(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var password = configuration["Admin:Password"];
        _password = string.IsNullOrWhiteSpace(password) ? null : password;
    }

    public bool IsConfigured => _password is not null;

    public bool Check(string? attempt)
    {
        if (_password is null || string.IsNullOrEmpty(attempt))
        {
            return false;
        }

        var actual = Encoding.UTF8.GetBytes(_password);
        var given = Encoding.UTF8.GetBytes(attempt);
        if (actual.Length != given.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(actual, given);
    }
}
