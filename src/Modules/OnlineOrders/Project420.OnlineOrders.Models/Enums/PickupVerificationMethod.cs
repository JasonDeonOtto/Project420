namespace Project420.OnlineOrders.Models.Enums;

/// <summary>
/// Method used to verify customer identity and age at pickup
/// Cannabis Act compliance requirement
/// </summary>
public enum PickupVerificationMethod
{
    /// <summary>
    /// South African ID document
    /// </summary>
    IDDocument = 1,

    /// <summary>
    /// Passport
    /// </summary>
    Passport = 2,

    /// <summary>
    /// Driver's license
    /// </summary>
    DriversLicense = 3,

    /// <summary>
    /// Other government-issued ID
    /// </summary>
    OtherGovernmentID = 99
}
