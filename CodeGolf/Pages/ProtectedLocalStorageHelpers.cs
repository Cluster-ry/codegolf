using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CodeGolf.Pages;

internal static class ProtectedLocalStorageHelpers
{
    public static async Task<T?> TryGetAsync<T>(ProtectedLocalStorage storage, string key, ILogger logger)
    {
        try
        {
            var result = await storage.GetAsync<T>(key);
            return result.Success ? result.Value : default;
        }
        catch (CryptographicException ex)
        {
            logger.LogWarning(ex, "Could not decrypt protected local storage key {Key}; clearing stale value.", key);
            try
            {
                await storage.DeleteAsync(key);
            }
            catch (Exception deleteEx)
            {
                logger.LogDebug(deleteEx, "Failed to delete stale protected local storage key {Key}.", key);
            }

            return default;
        }
    }
}
