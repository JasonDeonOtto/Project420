namespace Project420.UI.Maui.Infrastructure.Services;

/// <summary>
/// Implementation of ISecureStorageService using MAUI's SecureStorage API.
/// </summary>
/// <remarks>
/// Platform-specific implementations:
/// - iOS: Stored in the Keychain
/// - Android: Encrypted and stored using the KeyStore
/// - Windows: Encrypted and stored using Data Protection API
/// </remarks>
public class SecureStorageService : ISecureStorageService
{
    /// <summary>
    /// Securely stores a string value.
    /// </summary>
    public async Task SetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch (Exception ex)
        {
            // Log the error (inject ILogger if needed)
            System.Diagnostics.Debug.WriteLine($"Error setting secure storage for key '{key}': {ex.Message}");
            throw new InvalidOperationException($"Failed to store value securely for key '{key}'", ex);
        }
    }

    /// <summary>
    /// Retrieves a securely stored string value.
    /// </summary>
    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch (Exception ex)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error getting secure storage for key '{key}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Removes a securely stored value.
    /// </summary>
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            return await Task.Run(() =>
            {
                return SecureStorage.Default.Remove(key);
            });
        }
        catch (Exception ex)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error removing secure storage for key '{key}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clears all securely stored values.
    /// </summary>
    public async Task RemoveAllAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                SecureStorage.Default.RemoveAll();
            });
        }
        catch (Exception ex)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error clearing all secure storage: {ex.Message}");
            throw new InvalidOperationException("Failed to clear secure storage", ex);
        }
    }

    /// <summary>
    /// Checks if a value exists for the specified key.
    /// </summary>
    public async Task<bool> ContainsKeyAsync(string key)
    {
        try
        {
            var value = await SecureStorage.Default.GetAsync(key);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
