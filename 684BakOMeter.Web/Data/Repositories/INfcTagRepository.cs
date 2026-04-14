using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Data.Repositories;

/// <summary>Defines async operations for NFC tag persistence.</summary>
public interface INfcTagRepository
{
    /// <summary>Find an NFC tag by its hardware UID.</summary>
    Task<NfcTag?> GetByUidAsync(string uid);

    /// <summary>Get a single tag by primary key.</summary>
    Task<NfcTag?> GetByIdAsync(int id);

    /// <summary>Get all active NFC tags for a player.</summary>
    Task<IEnumerable<NfcTag>> GetByPlayerIdAsync(int playerId);

    /// <summary>Count active NFC tags for a player.</summary>
    Task<int> CountByPlayerIdAsync(int playerId);

    Task AddAsync(NfcTag tag);
    Task UpdateAsync(NfcTag tag);
    Task DeleteAsync(int id);
}
