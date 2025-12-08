using System.ComponentModel.DataAnnotations;

namespace Project420.OnlineOrders.BLL.DTOs.Request;

/// <summary>
/// DTO for customer login
/// </summary>
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remember me (longer session)
    /// </summary>
    public bool RememberMe { get; set; }
}
