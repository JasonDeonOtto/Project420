using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services;

/// <summary>
/// Service for validating South African ID numbers and extracting age information
/// </summary>
/// <remarks>
/// Phase 9.7: Age Verification Enhancement
///
/// SA ID Number Format: YYMMDDGSSSCAZ (13 digits)
/// - YYMMDD: Date of birth
/// - G: Gender (0-4 = female, 5-9 = male)
/// - SSS: Sequence number
/// - C: Citizenship (0 = SA citizen, 1 = permanent resident)
/// - A: Usually 8 (deprecated)
/// - Z: Luhn check digit
///
/// Cannabis Act Compliance:
/// - All sales require age verification (18+)
/// - ID scanning provides audit trail
/// - DOB extracted for accurate age calculation
/// </remarks>
public interface ISAIdValidationService
{
    /// <summary>
    /// Validates a South African ID number format and check digit
    /// </summary>
    /// <param name="idNumber">13-digit SA ID number</param>
    /// <returns>Validation result with extracted information</returns>
    SAIdValidationResultDto ValidateIdNumber(string idNumber);

    /// <summary>
    /// Extracts date of birth from a valid SA ID number
    /// </summary>
    /// <param name="idNumber">13-digit SA ID number</param>
    /// <returns>Date of birth or null if invalid</returns>
    DateTime? ExtractDateOfBirth(string idNumber);

    /// <summary>
    /// Calculates age from a valid SA ID number
    /// </summary>
    /// <param name="idNumber">13-digit SA ID number</param>
    /// <returns>Age in years or null if invalid</returns>
    int? CalculateAge(string idNumber);

    /// <summary>
    /// Checks if person is 18 or older based on ID number
    /// </summary>
    /// <param name="idNumber">13-digit SA ID number</param>
    /// <returns>True if 18+, false if under 18, null if invalid ID</returns>
    bool? IsOver18(string idNumber);

    /// <summary>
    /// Validates age from manually entered date of birth
    /// </summary>
    /// <param name="dateOfBirth">Customer's date of birth</param>
    /// <returns>Age verification result</returns>
    AgeVerificationResultDto ValidateManualDob(DateTime dateOfBirth);

    /// <summary>
    /// Extracts gender from SA ID number
    /// </summary>
    /// <param name="idNumber">13-digit SA ID number</param>
    /// <returns>Gender (Male/Female) or null if invalid</returns>
    string? ExtractGender(string idNumber);

    /// <summary>
    /// Extracts citizenship status from SA ID number
    /// </summary>
    /// <param name="idNumber">13-digit SA ID number</param>
    /// <returns>Citizenship status or null if invalid</returns>
    string? ExtractCitizenship(string idNumber);
}
