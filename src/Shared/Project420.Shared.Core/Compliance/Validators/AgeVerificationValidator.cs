namespace Project420.Shared.Core.Compliance.Validators;

/// <summary>
/// Validator for age verification (18+ requirement).
/// Ensures compliance with Cannabis for Private Purposes Act 2024.
/// </summary>
public static class AgeVerificationValidator
{
    /// <summary>
    /// Validates South African ID number date format (without checksum).
    /// Format: YYMMDD-SSSS-C-ZZ
    /// </summary>
    /// <param name="idNumber">SA ID number (13 digits)</param>
    /// <returns>True if valid date format, false otherwise</returns>
    public static bool IsValidSAIdNumberDateFormat(string? idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13)
            return false;

        if (!idNumber.All(char.IsDigit))
            return false;

        try
        {
            ExtractDateOfBirth(idNumber);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts date of birth from SA ID number.
    /// Format: YYMMDD-SSSS-C-ZZ
    /// </summary>
    /// <param name="idNumber">SA ID number (13 digits)</param>
    /// <returns>Date of birth</returns>
    /// <exception cref="ArgumentException">If ID number format is invalid</exception>
    public static DateTime ExtractDateOfBirth(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length < 6)
            throw new ArgumentException("Invalid ID number length", nameof(idNumber));

        var year = int.Parse(idNumber.Substring(0, 2));
        var month = int.Parse(idNumber.Substring(2, 2));
        var day = int.Parse(idNumber.Substring(4, 2));

        // Validate date ranges
        if (month < 1 || month > 12 || day < 1 || day > 31)
            throw new ArgumentException("Invalid date in ID number", nameof(idNumber));

        // Determine century (00-24 = 2000s, 25-99 = 1900s)
        var fullYear = year <= 24 ? 2000 + year : 1900 + year;

        try
        {
            return new DateTime(fullYear, month, day);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ArgumentException("Invalid date in ID number", nameof(idNumber), ex);
        }
    }

    /// <summary>
    /// Calculates age from date of birth.
    /// </summary>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>Age in years</returns>
    public static int CalculateAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;

        // Adjust if birthday hasn't occurred this year yet
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Validates that person is at least 18 years old.
    /// Cannabis Act requirement: 18+ years for cannabis purchase/possession.
    /// </summary>
    /// <param name="idNumber">SA ID number (13 digits)</param>
    /// <returns>True if 18 or older, false otherwise</returns>
    public static bool IsAtLeast18YearsOld(string? idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            return false;

        try
        {
            var dateOfBirth = ExtractDateOfBirth(idNumber);
            var age = CalculateAge(dateOfBirth);
            return age >= CannabisComplianceConstants.MIN_AGE_REQUIREMENT;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that person is at least 18 years old from date of birth.
    /// </summary>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>True if 18 or older, false otherwise</returns>
    public static bool IsAtLeast18YearsOld(DateTime dateOfBirth)
    {
        var age = CalculateAge(dateOfBirth);
        return age >= CannabisComplianceConstants.MIN_AGE_REQUIREMENT;
    }

    /// <summary>
    /// Validates that person meets minimum age requirement.
    /// </summary>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <param name="minimumAge">Minimum age requirement (default 18)</param>
    /// <returns>True if meets minimum age, false otherwise</returns>
    public static bool MeetsMinimumAge(DateTime dateOfBirth, int minimumAge = CannabisComplianceConstants.MIN_AGE_REQUIREMENT)
    {
        var age = CalculateAge(dateOfBirth);
        return age >= minimumAge;
    }
}
