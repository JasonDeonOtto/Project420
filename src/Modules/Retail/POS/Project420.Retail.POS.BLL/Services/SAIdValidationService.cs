using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services;

/// <summary>
/// Service for validating South African ID numbers and extracting age information
/// </summary>
/// <remarks>
/// Phase 9.7: Age Verification Enhancement
///
/// SA ID Number Format: YYMMDDGSSSCAZ (13 digits)
/// Position breakdown:
/// - 0-5 (YYMMDD): Date of birth
/// - 6 (G): Gender digit (0-4 = female, 5-9 = male)
/// - 7-9 (SSS): Sequence number for people born on same day
/// - 10 (C): Citizenship (0 = SA citizen, 1 = permanent resident)
/// - 11 (A): Usually 8 (deprecated race classification)
/// - 12 (Z): Luhn algorithm check digit
///
/// Example: 8501015009087
/// - DOB: 1985-01-01
/// - Gender: Male (5)
/// - Sequence: 009
/// - Citizen: SA Citizen (0)
/// - Check: 7
/// </remarks>
public class SAIdValidationService : ISAIdValidationService
{
    private const int ID_LENGTH = 13;
    private const int MINIMUM_AGE = 18;

    /// <inheritdoc/>
    public SAIdValidationResultDto ValidateIdNumber(string idNumber)
    {
        var result = new SAIdValidationResultDto
        {
            IdNumber = idNumber ?? string.Empty
        };

        // Basic format validation
        if (string.IsNullOrWhiteSpace(idNumber))
        {
            result.IsValid = false;
            result.Errors.Add("ID number is required");
            return result;
        }

        // Remove any spaces or dashes
        idNumber = idNumber.Replace(" ", "").Replace("-", "");
        result.IdNumber = idNumber;

        // Check length
        if (idNumber.Length != ID_LENGTH)
        {
            result.IsValid = false;
            result.Errors.Add($"ID number must be exactly {ID_LENGTH} digits (got {idNumber.Length})");
            return result;
        }

        // Check all digits
        if (!idNumber.All(char.IsDigit))
        {
            result.IsValid = false;
            result.Errors.Add("ID number must contain only digits");
            return result;
        }

        // Validate date of birth portion
        var dobResult = ValidateDateOfBirth(idNumber);
        if (!dobResult.IsValid)
        {
            result.IsValid = false;
            result.Errors.Add(dobResult.ErrorMessage!);
            return result;
        }

        // Validate Luhn check digit
        if (!ValidateLuhnCheckDigit(idNumber))
        {
            result.IsValid = false;
            result.Errors.Add("ID number failed check digit validation (invalid ID)");
            return result;
        }

        // ID is valid - extract all information
        result.IsValid = true;
        result.DateOfBirth = dobResult.DateOfBirth;
        result.Age = CalculateAgeFromDob(dobResult.DateOfBirth!.Value);
        result.IsOver18 = result.Age >= MINIMUM_AGE;
        result.Gender = ExtractGender(idNumber);
        result.Citizenship = ExtractCitizenship(idNumber);

        // Add warnings
        if (result.Age == MINIMUM_AGE)
        {
            // Check if they just turned 18 (within last 30 days)
            var birthday18 = dobResult.DateOfBirth!.Value.AddYears(MINIMUM_AGE);
            var daysSince18 = (DateTime.Today - birthday18).Days;
            if (daysSince18 <= 30)
            {
                result.Warnings.Add($"Customer turned 18 only {daysSince18} days ago - verify ID carefully");
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public DateTime? ExtractDateOfBirth(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            return null;

        idNumber = idNumber.Replace(" ", "").Replace("-", "");

        if (idNumber.Length != ID_LENGTH || !idNumber.All(char.IsDigit))
            return null;

        var dobResult = ValidateDateOfBirth(idNumber);
        return dobResult.IsValid ? dobResult.DateOfBirth : null;
    }

    /// <inheritdoc/>
    public int? CalculateAge(string idNumber)
    {
        var dob = ExtractDateOfBirth(idNumber);
        return dob.HasValue ? CalculateAgeFromDob(dob.Value) : null;
    }

    /// <inheritdoc/>
    public bool? IsOver18(string idNumber)
    {
        var age = CalculateAge(idNumber);
        return age.HasValue ? age >= MINIMUM_AGE : null;
    }

    /// <inheritdoc/>
    public AgeVerificationResultDto ValidateManualDob(DateTime dateOfBirth)
    {
        var result = new AgeVerificationResultDto
        {
            DateOfBirth = dateOfBirth.Date,
            VerificationDate = DateTime.UtcNow,
            Method = AgeVerificationMethod.ManualDob
        };

        // Check if DOB is in the future
        if (dateOfBirth.Date > DateTime.Today)
        {
            result.IsVerified = false;
            result.IsOver18 = false;
            result.ErrorMessage = "Date of birth cannot be in the future";
            return result;
        }

        // Check if DOB is unreasonably old (e.g., over 120 years)
        var age = CalculateAgeFromDob(dateOfBirth);
        if (age > 120)
        {
            result.IsVerified = false;
            result.IsOver18 = false;
            result.ErrorMessage = "Date of birth is invalid (age over 120)";
            return result;
        }

        result.Age = age;
        result.IsOver18 = age >= MINIMUM_AGE;
        result.IsVerified = true;

        if (!result.IsOver18)
        {
            result.ErrorMessage = $"Customer is {age} years old - must be {MINIMUM_AGE}+ for cannabis sales";
        }

        return result;
    }

    /// <inheritdoc/>
    public string? ExtractGender(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            return null;

        idNumber = idNumber.Replace(" ", "").Replace("-", "");

        if (idNumber.Length != ID_LENGTH || !idNumber.All(char.IsDigit))
            return null;

        // Position 6 is gender digit: 0-4 = female, 5-9 = male
        var genderDigit = int.Parse(idNumber[6].ToString());
        return genderDigit >= 5 ? "Male" : "Female";
    }

    /// <inheritdoc/>
    public string? ExtractCitizenship(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber))
            return null;

        idNumber = idNumber.Replace(" ", "").Replace("-", "");

        if (idNumber.Length != ID_LENGTH || !idNumber.All(char.IsDigit))
            return null;

        // Position 10 is citizenship: 0 = SA citizen, 1 = permanent resident
        var citizenDigit = idNumber[10];
        return citizenDigit == '0' ? "SA Citizen" : "Permanent Resident";
    }

    /// <summary>
    /// Validates the date of birth portion of the ID number
    /// </summary>
    private (bool IsValid, DateTime? DateOfBirth, string? ErrorMessage) ValidateDateOfBirth(string idNumber)
    {
        // Extract YYMMDD
        var yearPart = idNumber.Substring(0, 2);
        var monthPart = idNumber.Substring(2, 2);
        var dayPart = idNumber.Substring(4, 2);

        if (!int.TryParse(yearPart, out var year) ||
            !int.TryParse(monthPart, out var month) ||
            !int.TryParse(dayPart, out var day))
        {
            return (false, null, "Invalid date format in ID number");
        }

        // Determine century (SA IDs use 2-digit year)
        // Rule: If YY > current year's last 2 digits, assume 1900s, else 2000s
        var currentYearLastTwo = DateTime.Today.Year % 100;
        var fullYear = year > currentYearLastTwo ? 1900 + year : 2000 + year;

        // Validate month
        if (month < 1 || month > 12)
        {
            return (false, null, $"Invalid month in ID number: {month}");
        }

        // Validate day
        var maxDay = DateTime.DaysInMonth(fullYear, month);
        if (day < 1 || day > maxDay)
        {
            return (false, null, $"Invalid day in ID number: {day} for month {month}");
        }

        var dateOfBirth = new DateTime(fullYear, month, day);

        // Check if DOB is in the future
        if (dateOfBirth > DateTime.Today)
        {
            return (false, null, "Date of birth from ID is in the future - invalid ID");
        }

        return (true, dateOfBirth, null);
    }

    /// <summary>
    /// Validates the Luhn check digit (position 12)
    /// </summary>
    /// <remarks>
    /// SA ID uses standard Luhn mod-10 algorithm:
    /// 1. Starting from the right, double every second digit
    /// 2. If doubling results in > 9, subtract 9
    /// 3. Sum all digits
    /// 4. Valid if sum % 10 == 0
    /// </remarks>
    private bool ValidateLuhnCheckDigit(string idNumber)
    {
        int sum = 0;
        bool alternate = false;

        // Process from right to left
        for (int i = idNumber.Length - 1; i >= 0; i--)
        {
            int digit = int.Parse(idNumber[i].ToString());

            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    /// <summary>
    /// Calculates age from date of birth
    /// </summary>
    private int CalculateAgeFromDob(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;

        // Adjust if birthday hasn't occurred yet this year
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    /// <summary>
    /// Masks an ID number for privacy (shows DOB only)
    /// </summary>
    /// <param name="idNumber">Full ID number</param>
    /// <returns>Masked ID like "850101*****87"</returns>
    public static string MaskIdNumber(string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != ID_LENGTH)
            return "Invalid ID";

        // Show first 6 (DOB) and last 2 digits
        return idNumber.Substring(0, 6) + "*****" + idNumber.Substring(11, 2);
    }
}
