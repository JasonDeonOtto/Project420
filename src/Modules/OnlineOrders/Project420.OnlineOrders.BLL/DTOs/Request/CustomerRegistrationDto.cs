using System.ComponentModel.DataAnnotations;

namespace Project420.OnlineOrders.BLL.DTOs.Request;

/// <summary>
/// DTO for customer registration
/// </summary>
public class CustomerRegistrationDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string IdNumber { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// POPIA consent (REQUIRED)
    /// </summary>
    [Required]
    public bool ConsentToPOPIA { get; set; }

    /// <summary>
    /// Marketing consent (OPTIONAL)
    /// </summary>
    public bool ConsentToMarketing { get; set; }
}
