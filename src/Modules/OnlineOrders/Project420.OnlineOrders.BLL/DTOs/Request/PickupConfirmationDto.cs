using System.ComponentModel.DataAnnotations;
using Project420.OnlineOrders.Models.Enums;

namespace Project420.OnlineOrders.BLL.DTOs.Request;

/// <summary>
/// DTO for confirming order pickup with age verification
/// </summary>
public class PickupConfirmationDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public PickupVerificationMethod IdVerificationMethod { get; set; }

    [Required]
    [MaxLength(20)]
    public string IdNumberVerified { get; set; } = string.Empty;

    [Required]
    public bool AgeConfirmed { get; set; }

    [Required]
    public int VerifiedByStaffId { get; set; }

    [MaxLength(1000)]
    public string? VerificationNotes { get; set; }
}
