using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace _684BakOMeter.Tests.Services;

public class NfcServiceTests
{
    private readonly INfcTagRepository _nfcTags = Substitute.For<INfcTagRepository>();
    private readonly IPlayerRepository _players = Substitute.For<IPlayerRepository>();
    private readonly ILogger<NfcService> _logger = Substitute.For<ILogger<NfcService>>();
    private readonly NfcService _sut;

    public NfcServiceTests()
    {
        _sut = new NfcService(_nfcTags, _players, _logger);
    }

    // --- ResolveTagAsync ---

    [Fact]
    public async Task ResolveTagAsync_UnknownUid_ReturnsNotKnown()
    {
        _nfcTags.GetByUidAsync(Arg.Any<string>()).Returns((NfcTag?)null);

        var (tag, player, isKnown) = await _sut.ResolveTagAsync("AABB1122");

        Assert.False(isKnown);
        Assert.Null(tag);
        Assert.Null(player);
    }

    [Fact]
    public async Task ResolveTagAsync_InactiveTag_ReturnsNotKnown()
    {
        var hash = NfcUidHasher.Hash("AABB1122");
        _nfcTags.GetByUidAsync(hash).Returns(new NfcTag { Uid = hash, IsActive = false });

        var (tag, player, isKnown) = await _sut.ResolveTagAsync("AABB1122");

        Assert.False(isKnown);
        Assert.NotNull(tag);
        Assert.Null(player);
    }

    [Fact]
    public async Task ResolveTagAsync_ActiveTag_ReturnsKnownWithPlayer()
    {
        var hash = NfcUidHasher.Hash("AABB1122");
        var expected = new Player { Id = 42, Name = "test" };
        _nfcTags.GetByUidAsync(hash).Returns(new NfcTag { Uid = hash, IsActive = true, Player = expected });

        var (tag, player, isKnown) = await _sut.ResolveTagAsync("AABB1122");

        Assert.True(isKnown);
        Assert.Equal(42, player!.Id);
    }

    [Fact]
    public async Task ResolveTagAsync_HashesUidBeforeLookup()
    {
        await _sut.ResolveTagAsync("  aabb1122  ");

        var expectedHash = NfcUidHasher.Hash("aabb1122");
        await _nfcTags.Received(1).GetByUidAsync(expectedHash);
    }

    // --- CreateUserFromNfcAsync ---

    [Fact]
    public async Task CreateUserFromNfcAsync_EmptyName_ReturnsError()
    {
        var (_, _, error) = await _sut.CreateUserFromNfcAsync("  ", "AABB");
        Assert.NotNull(error);
    }

    [Fact]
    public async Task CreateUserFromNfcAsync_DuplicateName_ReturnsError()
    {
        _players.GetByNameAsync("vincent").Returns(new Player { Name = "vincent" });

        var (_, _, error) = await _sut.CreateUserFromNfcAsync("Vincent", "AABB");

        Assert.Equal("Deze naam is al in gebruik.", error);
    }

    [Fact]
    public async Task CreateUserFromNfcAsync_DuplicateTag_ReturnsError()
    {
        _players.GetByNameAsync(Arg.Any<string>()).Returns((Player?)null);
        _nfcTags.GetByUidAsync(Arg.Any<string>()).Returns(new NfcTag());

        var (_, _, error) = await _sut.CreateUserFromNfcAsync("NewPlayer", "AABB");

        Assert.Equal("Deze NFC tag is al gekoppeld aan een andere speler.", error);
    }

    [Fact]
    public async Task CreateUserFromNfcAsync_Success_CreatesPlayerAndTag()
    {
        _players.GetByNameAsync(Arg.Any<string>()).Returns((Player?)null);
        _nfcTags.GetByUidAsync(Arg.Any<string>()).Returns((NfcTag?)null);

        var (player, tag, error) = await _sut.CreateUserFromNfcAsync("NewPlayer", "AABB");

        Assert.Null(error);
        Assert.NotNull(player);
        Assert.NotNull(tag);
        Assert.True(tag.IsActive);
        await _players.Received(1).AddAsync(Arg.Any<Player>());
        await _nfcTags.Received(1).AddAsync(Arg.Any<NfcTag>());
    }

    // --- LinkTagToPlayerAsync ---

    [Fact]
    public async Task LinkTagToPlayerAsync_TagAlreadyLinkedToSamePlayer_ReturnsError()
    {
        var hash = NfcUidHasher.Hash("AABB");
        _nfcTags.GetByUidAsync(hash).Returns(new NfcTag { PlayerId = 1 });

        var (_, error) = await _sut.LinkTagToPlayerAsync(1, "AABB");

        Assert.Equal("Deze tag is al gekoppeld aan jouw account.", error);
    }

    [Fact]
    public async Task LinkTagToPlayerAsync_TagLinkedToOtherPlayer_ReturnsError()
    {
        var hash = NfcUidHasher.Hash("AABB");
        _nfcTags.GetByUidAsync(hash).Returns(new NfcTag { PlayerId = 99 });

        var (_, error) = await _sut.LinkTagToPlayerAsync(1, "AABB");

        Assert.Equal("Deze tag is al gekoppeld aan een andere speler.", error);
    }

    [Fact]
    public async Task LinkTagToPlayerAsync_PlayerNotFound_ReturnsError()
    {
        _nfcTags.GetByUidAsync(Arg.Any<string>()).Returns((NfcTag?)null);
        _players.GetByIdAsync(1).Returns((Player?)null);

        var (_, error) = await _sut.LinkTagToPlayerAsync(1, "AABB");

        Assert.Equal("Speler niet gevonden.", error);
    }

    [Fact]
    public async Task LinkTagToPlayerAsync_Success_CreatesTag()
    {
        _nfcTags.GetByUidAsync(Arg.Any<string>()).Returns((NfcTag?)null);
        _players.GetByIdAsync(1).Returns(new Player { Id = 1, Name = "test" });

        var (tag, error) = await _sut.LinkTagToPlayerAsync(1, "AABB");

        Assert.Null(error);
        Assert.NotNull(tag);
        Assert.Equal(1, tag.PlayerId);
        await _nfcTags.Received(1).AddAsync(Arg.Any<NfcTag>());
    }

    // --- RemoveTagAsync ---

    [Fact]
    public async Task RemoveTagAsync_TagNotFound_ReturnsError()
    {
        _nfcTags.GetByIdAsync(1).Returns((NfcTag?)null);

        var error = await _sut.RemoveTagAsync(1, 1);

        Assert.Equal("Tag niet gevonden.", error);
    }

    [Fact]
    public async Task RemoveTagAsync_TagBelongsToOtherPlayer_ReturnsError()
    {
        _nfcTags.GetByIdAsync(1).Returns(new NfcTag { Id = 1, PlayerId = 99 });

        var error = await _sut.RemoveTagAsync(1, 1);

        Assert.Equal("Deze tag hoort niet bij jouw account.", error);
    }

    [Fact]
    public async Task RemoveTagAsync_LastTag_ReturnsError()
    {
        _nfcTags.GetByIdAsync(1).Returns(new NfcTag { Id = 1, PlayerId = 1 });
        _nfcTags.CountByPlayerIdAsync(1).Returns(1);

        var error = await _sut.RemoveTagAsync(1, 1);

        Assert.Equal("Je kunt je laatste NFC tag niet verwijderen.", error);
    }

    [Fact]
    public async Task RemoveTagAsync_MultipleTags_DeletesSuccessfully()
    {
        _nfcTags.GetByIdAsync(1).Returns(new NfcTag { Id = 1, PlayerId = 1 });
        _nfcTags.CountByPlayerIdAsync(1).Returns(2);

        var error = await _sut.RemoveTagAsync(1, 1);

        Assert.Null(error);
        await _nfcTags.Received(1).DeleteAsync(1);
    }
}
