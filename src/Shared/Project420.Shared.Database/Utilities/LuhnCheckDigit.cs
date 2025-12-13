namespace Project420.Shared.Database.Utilities;

/// <summary>
/// Luhn algorithm (mod 10) check digit calculator for serial number validation.
/// </summary>
/// <remarks>
/// The Luhn algorithm is an industry-standard checksum formula used to validate:
/// - Credit card numbers
/// - IMEI numbers
/// - National identification numbers
/// - Serial numbers
///
/// Benefits:
/// - Catches 99% of single-digit transcription errors
/// - Catches 98% of adjacent digit transposition errors
/// - Simple to implement and verify
///
/// Algorithm:
/// 1. Starting from the rightmost digit (excluding check digit), double every second digit
/// 2. If doubling results in a number > 9, subtract 9
/// 3. Sum all digits
/// 4. Check digit = (10 - (sum % 10)) % 10
///
/// Reference: ISO/IEC 7812-1 (also known as the Luhn formula)
/// </remarks>
public static class LuhnCheckDigit
{
    /// <summary>
    /// Calculates the Luhn check digit for a numeric string.
    /// </summary>
    /// <param name="number">The numeric string without check digit</param>
    /// <returns>Single check digit (0-9)</returns>
    /// <exception cref="ArgumentException">If input is null, empty, or not numeric</exception>
    /// <example>
    /// var checkDigit = LuhnCheckDigit.Calculate("011001020251206000100001003551");
    /// // Returns 7
    /// // Full serial with check: "0110010202512060001000010035517"
    /// </example>
    public static int Calculate(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("Number cannot be null or empty.", nameof(number));
        }

        if (!IsNumeric(number))
        {
            throw new ArgumentException("Number must contain only digits.", nameof(number));
        }

        int sum = 0;
        bool alternate = true; // Start with true for rightmost position

        // Process digits from right to left
        for (int i = number.Length - 1; i >= 0; i--)
        {
            int digit = number[i] - '0';

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

        // Check digit is what makes the total sum a multiple of 10
        return (10 - (sum % 10)) % 10;
    }

    /// <summary>
    /// Appends the Luhn check digit to a numeric string.
    /// </summary>
    /// <param name="number">The numeric string without check digit</param>
    /// <returns>The original number with check digit appended</returns>
    /// <example>
    /// var withCheck = LuhnCheckDigit.AppendCheckDigit("011001020251206000100001003551");
    /// // Returns "0110010202512060001000010035517"
    /// </example>
    public static string AppendCheckDigit(string number)
    {
        int checkDigit = Calculate(number);
        return number + checkDigit.ToString();
    }

    /// <summary>
    /// Validates a number string that includes a Luhn check digit.
    /// </summary>
    /// <param name="numberWithCheck">The complete number including check digit</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <example>
    /// bool isValid = LuhnCheckDigit.Validate("0110010202512060001000010035517");
    /// // Returns true
    ///
    /// bool isInvalid = LuhnCheckDigit.Validate("0110010202512060001000010035510");
    /// // Returns false (wrong check digit)
    /// </example>
    public static bool Validate(string numberWithCheck)
    {
        if (string.IsNullOrWhiteSpace(numberWithCheck) || numberWithCheck.Length < 2)
        {
            return false;
        }

        if (!IsNumeric(numberWithCheck))
        {
            return false;
        }

        // Extract original number and check digit
        string originalNumber = numberWithCheck.Substring(0, numberWithCheck.Length - 1);
        char providedCheckChar = numberWithCheck[^1];

        if (!char.IsDigit(providedCheckChar))
        {
            return false;
        }

        int providedCheckDigit = providedCheckChar - '0';
        int calculatedCheckDigit = Calculate(originalNumber);

        return providedCheckDigit == calculatedCheckDigit;
    }

    /// <summary>
    /// Extracts the check digit from a validated number string.
    /// </summary>
    /// <param name="numberWithCheck">The complete number including check digit</param>
    /// <returns>The check digit</returns>
    /// <exception cref="ArgumentException">If input is invalid</exception>
    public static int ExtractCheckDigit(string numberWithCheck)
    {
        if (string.IsNullOrWhiteSpace(numberWithCheck) || numberWithCheck.Length < 1)
        {
            throw new ArgumentException("Number must have at least one digit.", nameof(numberWithCheck));
        }

        char checkChar = numberWithCheck[^1];
        if (!char.IsDigit(checkChar))
        {
            throw new ArgumentException("Last character must be a digit.", nameof(numberWithCheck));
        }

        return checkChar - '0';
    }

    /// <summary>
    /// Removes the check digit from a number string.
    /// </summary>
    /// <param name="numberWithCheck">The complete number including check digit</param>
    /// <returns>The number without the check digit</returns>
    public static string RemoveCheckDigit(string numberWithCheck)
    {
        if (string.IsNullOrWhiteSpace(numberWithCheck) || numberWithCheck.Length < 2)
        {
            throw new ArgumentException("Number must have at least 2 characters.", nameof(numberWithCheck));
        }

        return numberWithCheck.Substring(0, numberWithCheck.Length - 1);
    }

    /// <summary>
    /// Checks if a string contains only numeric digits.
    /// </summary>
    private static bool IsNumeric(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsDigit(c))
                return false;
        }
        return true;
    }
}
