using System.Security.Cryptography;
using System.Text;

namespace JJManager.Core.Others;

/// <summary>
/// Helper for generating deterministic hashes
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Generates a deterministic hash from a string using SHA256.
    /// Returns a 64-character hex string that is always the same for the same input.
    /// </summary>
    public static string GetDeterministicHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
