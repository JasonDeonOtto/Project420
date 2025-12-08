namespace Project420.OnlineOrders.Models.Enums;

/// <summary>
/// Payment method used by customer
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Credit/Debit card
    /// </summary>
    Card = 1,

    /// <summary>
    /// Electronic Funds Transfer
    /// </summary>
    EFT = 2,

    /// <summary>
    /// Instant EFT
    /// </summary>
    InstantEFT = 3,

    /// <summary>
    /// SnapScan
    /// </summary>
    SnapScan = 4,

    /// <summary>
    /// Zapper
    /// </summary>
    Zapper = 5,

    /// <summary>
    /// Cash (in-store)
    /// </summary>
    Cash = 99
}
