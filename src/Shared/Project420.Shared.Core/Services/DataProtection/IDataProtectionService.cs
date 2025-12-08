namespace Project420.Shared.Core.Services.DataProtection;

/// <summary>
/// Service for masking and protecting personal information in compliance with POPIA.
/// Used across all modules to ensure consistent data protection.
/// </summary>
/// <remarks>
/// POPIA Compliance: Protection of Personal Information Act (2013)
/// - Enforces data minimization principle
/// - Reduces exposure of sensitive personal information
/// - Provides consistent masking across all UI components
/// - Supports audit-ready data protection patterns
///
/// Usage: Inject into Blazor components, services, or repositories that display PII.
/// </remarks>
public interface IDataProtectionService
{
    /// <summary>
    /// Masks a South African ID number, showing only the last 2 digits.
    /// </summary>
    /// <param name="idNumber">Full SA ID number (format: YYMMDD-SSSS-C-ZZ)</param>
    /// <returns>Masked ID number (format: ****-****-*-44)</returns>
    /// <remarks>
    /// ID numbers are POPIA "Special Personal Information" requiring extra protection.
    /// </remarks>
    string MaskIdNumber(string? idNumber);

    /// <summary>
    /// Masks a mobile phone number, showing only the last 4 digits.
    /// </summary>
    /// <param name="mobile">Full mobile number (format: 0XX XXX XXXX)</param>
    /// <returns>Masked mobile number (format: *** *** 1234)</returns>
    string MaskMobile(string? mobile);

    /// <summary>
    /// Masks an email address, showing only the first character and domain.
    /// </summary>
    /// <param name="email">Full email address (format: john@example.com)</param>
    /// <returns>Masked email address (format: j***@example.com)</returns>
    string MaskEmail(string? email);

    /// <summary>
    /// Masks a physical address, showing only the suburb/area.
    /// </summary>
    /// <param name="address">Full physical address</param>
    /// <returns>Masked address (format: *** Suburb, City)</returns>
    string MaskAddress(string? address);

    /// <summary>
    /// Masks a bank account number, showing only the last 4 digits.
    /// </summary>
    /// <param name="accountNumber">Full bank account number</param>
    /// <returns>Masked account number (format: **** 1234)</returns>
    string MaskBankAccount(string? accountNumber);

    /// <summary>
    /// Masks a medical license/permit number, showing only the last 4 characters.
    /// </summary>
    /// <param name="permitNumber">Full medical permit number (SAHPRA Section 21/22)</param>
    /// <returns>Masked permit number (format: ****-****-1234)</returns>
    /// <remarks>
    /// Medical information is POPIA "Special Personal Information".
    /// </remarks>
    string MaskMedicalPermit(string? permitNumber);

    /// <summary>
    /// Masks a financial amount for general display (e.g., credit limit, balance).
    /// </summary>
    /// <param name="amount">The financial amount</param>
    /// <param name="showFullAmount">If true, shows full amount. If false, shows masked amount (e.g., R ****)</param>
    /// <returns>Formatted/masked amount</returns>
    string MaskFinancialAmount(decimal amount, bool showFullAmount = false);

    /// <summary>
    /// Unmasks data with audit trail (future enhancement).
    /// </summary>
    /// <param name="maskedValue">The masked value</param>
    /// <param name="originalValue">The original unmasked value</param>
    /// <param name="userId">User requesting unmask</param>
    /// <param name="reason">Business reason for unmasking</param>
    /// <returns>Unmasked value with audit log created</returns>
    /// <remarks>
    /// Future enhancement: Will log all unmask operations for POPIA audit compliance.
    /// Requires role-based authorization.
    /// </remarks>
    Task<string> UnmaskWithAuditAsync(string maskedValue, string originalValue, int userId, string reason);
}
