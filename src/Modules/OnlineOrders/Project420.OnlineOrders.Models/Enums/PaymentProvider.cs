namespace Project420.OnlineOrders.Models.Enums;

/// <summary>
/// Supported payment providers in South Africa
/// </summary>
public enum PaymentProvider
{
    /// <summary>
    /// Yoco payment gateway (card payments)
    /// </summary>
    Yoco = 1,

    /// <summary>
    /// PayFast payment gateway (card, EFT, SnapScan, Zapper)
    /// </summary>
    PayFast = 2,

    /// <summary>
    /// Ozow instant EFT
    /// </summary>
    Ozow = 3,

    /// <summary>
    /// Manual/Cash payment (in-store)
    /// </summary>
    Manual = 99
}
