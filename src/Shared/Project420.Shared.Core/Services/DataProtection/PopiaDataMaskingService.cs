namespace Project420.Shared.Core.Services.DataProtection;

/// <summary>
/// Implementation of POPIA-compliant data masking service.
/// Provides consistent personal information protection across the entire application.
/// </summary>
/// <remarks>
/// POPIA Compliance Notes:
/// - Data Minimization: Only display necessary information in list views
/// - Security Safeguards: Reduce exposure of sensitive personal information
/// - Over-the-shoulder Protection: Prevents casual viewing of PII
/// - Audit Ready: Centralized masking makes compliance audits easier
///
/// Penalties for POPIA Non-Compliance:
/// - Administrative fine: Up to R10 million
/// - Serious violations: Up to 10 years imprisonment
/// - Civil liability: Compensation to affected individuals
/// </remarks>
public class PopiaDataMaskingService : IDataProtectionService
{
    private const string DefaultMaskedValue = "-";
    private const char MaskCharacter = '*';

    /// <inheritdoc />
    public string MaskIdNumber(string? idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            return DefaultMaskedValue;

        // Remove any spaces or dashes for processing
        var cleaned = idNumber.Replace(" ", "").Replace("-", "");

        // SA ID is 13 digits: YYMMDD SSSS C ZZ
        if (cleaned.Length < 2)
            return DefaultMaskedValue;

        // Show only last 2 digits: ****-****-*-44
        var lastTwo = cleaned.Substring(cleaned.Length - 2);
        return $"****-****-*-{lastTwo}";
    }

    /// <inheritdoc />
    public string MaskMobile(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return DefaultMaskedValue;

        // Remove spaces and formatting
        var cleaned = mobile.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // South African mobile: 10 digits (0XX XXX XXXX)
        if (cleaned.Length < 4)
            return DefaultMaskedValue;

        // Show only last 4 digits: *** *** 1234
        var lastFour = cleaned.Substring(cleaned.Length - 4);
        return $"*** *** {lastFour}";
    }

    /// <inheritdoc />
    public string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return DefaultMaskedValue;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return DefaultMaskedValue;

        var localPart = parts[0];
        var domain = parts[1];

        // Show first character only: j***@example.com
        var masked = localPart.Length <= 1
            ? localPart
            : $"{localPart[0]}{new string(MaskCharacter, 3)}";

        return $"{masked}@{domain}";
    }

    /// <inheritdoc />
    public string MaskAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return DefaultMaskedValue;

        // Strategy: Try to extract suburb/city from address
        // Common SA format: "123 Main St, Suburb, City, Postal Code"
        var parts = address.Split(',');

        if (parts.Length >= 2)
        {
            // Mask street number/name, show suburb onwards
            var visibleParts = parts.Skip(1).Select(p => p.Trim()).ToArray();
            return $"*** {string.Join(", ", visibleParts)}";
        }

        // Fallback: Show only last part of address
        return $"*** {parts[^1].Trim()}";
    }

    /// <inheritdoc />
    public string MaskBankAccount(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return DefaultMaskedValue;

        var cleaned = accountNumber.Replace(" ", "").Replace("-", "");

        if (cleaned.Length < 4)
            return DefaultMaskedValue;

        // Show last 4 digits: **** 1234
        var lastFour = cleaned.Substring(cleaned.Length - 4);
        return $"**** {lastFour}";
    }

    /// <inheritdoc />
    public string MaskMedicalPermit(string? permitNumber)
    {
        if (string.IsNullOrWhiteSpace(permitNumber))
            return DefaultMaskedValue;

        var cleaned = permitNumber.Replace(" ", "").Replace("-", "");

        if (cleaned.Length < 4)
            return DefaultMaskedValue;

        // SAHPRA permits vary in format
        // Show last 4 characters: ****-****-1234
        var lastFour = cleaned.Substring(cleaned.Length - 4);

        // If original had dashes, maintain some structure
        return permitNumber.Contains('-')
            ? $"****-****-{lastFour}"
            : $"****{lastFour}";
    }

    /// <inheritdoc />
    public string MaskFinancialAmount(decimal amount, bool showFullAmount = false)
    {
        if (showFullAmount)
            return $"R {amount:N2}";

        // For list views, mask the actual amount
        // Show only range indicators
        if (amount == 0)
            return "R 0.00";

        if (amount < 1000)
            return "R ***";

        if (amount < 10000)
            return "R *,***";

        if (amount < 100000)
            return "R **,***";

        return "R ***,***+";
    }

    /// <inheritdoc />
    public async Task<string> UnmaskWithAuditAsync(string maskedValue, string originalValue, int userId, string reason)
    {
        // TODO: Future enhancement - implement audit logging
        // This will log:
        // - Who unmasked the data (userId)
        // - What data was unmasked (type, masked/original values)
        // - When it was unmasked (timestamp)
        // - Why it was unmasked (business reason)
        // - Where it was unmasked (module, component)
        //
        // This audit trail is critical for POPIA compliance demonstrations.
        //
        // Implementation will:
        // 1. Verify user has permission to unmask (role-based authorization)
        // 2. Create immutable audit log entry
        // 3. Return original unmasked value
        // 4. Optionally: Notify data protection officer of sensitive unmask operations

        await Task.CompletedTask; // Placeholder for async audit logging

        // For now, return original value
        // In production, this would throw UnauthorizedException if user lacks permission
        return originalValue;
    }
}
