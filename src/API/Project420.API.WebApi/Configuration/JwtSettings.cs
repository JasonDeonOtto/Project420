namespace Project420.API.WebApi.Configuration;

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key for signing tokens (MUST be at least 32 characters)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (e.g., "Project420.API")
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Token audience (e.g., "Project420.Clients")
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration in minutes (default: 60 minutes)
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiration in days (default: 7 days)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
