namespace Project420.UI.Maui.Infrastructure.Services;

/// <summary>
/// Service for securely storing and retrieving sensitive data.
/// Uses platform-specific secure storage mechanisms (Keychain on iOS, KeyStore on Android).
/// </summary>
/// <remarks>
/// Use this service for storing sensitive data like:
/// - Authentication tokens (JWT, refresh tokens)
/// - API keys
/// - User credentials
/// - Encryption keys
/// Do NOT use for general application settings - use Preferences for that.
/// </remarks>
public interface ISecureStorageService
{
    /// <summary>
    /// Securely stores a string value with the specified key.
    /// </summary>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    Task SetAsync(string key, string value);

    /// <summary>
    /// Retrieves a securely stored string value by key.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>The stored value, or null if not found.</returns>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Removes a securely stored value by key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// Clears all securely stored values.
    /// Use with caution - this will log out the user and clear all tokens.
    /// </summary>
    Task RemoveAllAsync();

    /// <summary>
    /// Checks if a value exists for the specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if a value exists, false otherwise.</returns>
    Task<bool> ContainsKeyAsync(string key);
}
