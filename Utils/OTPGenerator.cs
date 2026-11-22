using System.Security.Cryptography;

namespace CO4029_BE.Utils;

public static class OTPGenerator
{
    /// <summary>
    /// Generates a numeric one-time code with a cryptographically secure RNG. Every digit 0–9 is
    /// equally likely (the previous implementation used <c>Random.Next(0, 9)</c>, which never produced 9).
    /// </summary>
    public static string GenerateOTP(int length = 4)
    {
        if (length < 1)
            throw new ArgumentOutOfRangeException(nameof(length));
        var digits = new char[length];
        for (int i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));
        }
        return new string(digits);
    }
}
