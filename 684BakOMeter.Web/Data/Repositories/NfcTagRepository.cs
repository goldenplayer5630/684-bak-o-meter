using _684BakOMeter.Web.Data.Persistence;
using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _684BakOMeter.Web.Data.Repositories;

public class NfcTagRepository : INfcTagRepository
{
    private readonly AppDbContext _db;

    public NfcTagRepository(AppDbContext db) => _db = db;

    public async Task<NfcTag?> GetByUidAsync(string uid)
        => await _db.NfcTags
                    .Include(t => t.Player)
                    .FirstOrDefaultAsync(t => t.Uid == uid);

    public async Task<NfcTag?> GetByIdAsync(int id)
        => await _db.NfcTags
                    .Include(t => t.Player)
                    .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<NfcTag>> GetByPlayerIdAsync(int playerId)
        => await _db.NfcTags
                    .Where(t => t.PlayerId == playerId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

    public async Task<int> CountByPlayerIdAsync(int playerId)
        => await _db.NfcTags
                    .CountAsync(t => t.PlayerId == playerId && t.IsActive);

    public async Task AddAsync(NfcTag tag)
    {
        _db.NfcTags.Add(tag);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(NfcTag tag)
    {
        _db.NfcTags.Update(tag);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await _db.NfcTags.FindAsync(id);
        if (tag is not null)
        {
            _db.NfcTags.Remove(tag);
            await _db.SaveChangesAsync();
        }
    }
}
