using _684BakOMeter.Web.Services;

namespace _684BakOMeter.Tests.Services;

public class NfcUidHasherTests
{
    [Fact]
    public void Hash_ReturnsDeterministicResult()
    {
        var hash1 = NfcUidHasher.Hash("AABBCCDD");
        var hash2 = NfcUidHasher.Hash("AABBCCDD");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_TrimsWhitespace()
    {
        var hash1 = NfcUidHasher.Hash("AABBCCDD");
        var hash2 = NfcUidHasher.Hash("  AABBCCDD  ");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_IsCaseInsensitive()
    {
        var hash1 = NfcUidHasher.Hash("aabbccdd");
        var hash2 = NfcUidHasher.Hash("AABBCCDD");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_DifferentUids_ProduceDifferentHashes()
    {
        var hash1 = NfcUidHasher.Hash("AABBCCDD");
        var hash2 = NfcUidHasher.Hash("11223344");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_ReturnsLowercaseHex()
    {
        var hash = NfcUidHasher.Hash("AABBCCDD");
        Assert.Matches("^[0-9a-f]{64}$", hash); // SHA-256 = 64 hex chars
    }

    [Fact]
    public void Hash_DoesNotReturnRawUid()
    {
        var hash = NfcUidHasher.Hash("AABBCCDD");
        Assert.DoesNotContain("AABBCCDD", hash, StringComparison.OrdinalIgnoreCase);
    }
}
