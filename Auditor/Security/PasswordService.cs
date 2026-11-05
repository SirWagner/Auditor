using System.Security.Cryptography;
using System.Text;
using Auditor.Models;
using Microsoft.AspNetCore.Identity;

namespace Auditor.Security
{
    /// <summary>
    /// Hashes passwords with ASP.NET Core Identity's PBKDF2 hasher. Hashes created by the original
    /// unsalted SHA-256 scheme still verify, and are flagged for re-hashing on the next successful sign-in.
    /// </summary>
    public class PasswordService
    {
        public const int MinimumLength = 8;

        private readonly PasswordHasher<AppUser> _hasher = new();

        public string Hash(AppUser user, string password) => _hasher.HashPassword(user, password);

        public PasswordCheck Verify(AppUser user, string password)
        {
            if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(password))
                return PasswordCheck.Failed;

            if (IsLegacyHash(user.PasswordHash))
            {
                var legacy = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
                return CryptographicOperations.FixedTimeEquals(
                        Encoding.ASCII.GetBytes(legacy), Encoding.ASCII.GetBytes(user.PasswordHash))
                    ? PasswordCheck.SuccessRehashNeeded
                    : PasswordCheck.Failed;
            }

            return _hasher.VerifyHashedPassword(user, user.PasswordHash, password) switch
            {
                PasswordVerificationResult.Success => PasswordCheck.Success,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordCheck.SuccessRehashNeeded,
                _ => PasswordCheck.Failed,
            };
        }

        /// <summary>Returns an error message, or null when the password is acceptable.</summary>
        public static string? Validate(string? password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinimumLength)
                return $"Password must be at least {MinimumLength} characters.";
            if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
                return "Password must contain at least one letter and one number.";
            return null;
        }

        // SHA-256 base64 is always 44 characters; Identity V3 hashes are much longer.
        private static bool IsLegacyHash(string hash) => hash.Length == 44;
    }

    public enum PasswordCheck
    {
        Failed,
        Success,
        SuccessRehashNeeded,
    }
}
