using System.Security.Cryptography;
using System.Text;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Hashes NFC tag UIDs with a fixed salt so raw UIDs are never stored.
/// </summary>
public static class NfcUidHasher
{
    private const string Salt = "6f2b8fe835ff811f1ab6566e976c7393f83dfad2";

    /// <summary>
    /// Returns a SHA-256 hex digest of the salted UID.
    /// The UID is trimmed and upper-cased before hashing.
    /// </summary>
    public static string Hash(string uid)
    {
        var normalized = uid.Trim().ToUpperInvariant();
        var input = Encoding.UTF8.GetBytes(Salt + normalized);
        var hash = SHA256.HashData(input);
        return Convert.ToHexStringLower(hash);
    }
}
