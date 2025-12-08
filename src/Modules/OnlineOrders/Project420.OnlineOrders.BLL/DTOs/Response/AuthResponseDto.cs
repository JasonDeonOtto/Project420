namespace Project420.OnlineOrders.BLL.DTOs.Response;

/// <summary>
/// DTO for authentication response
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public int CustomerId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public bool AgeVerified { get; set; }
    public bool RequiresIDVerification { get; set; }
    public string? ErrorMessage { get; set; }
}
