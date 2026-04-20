using _684BakOMeter.Web.Data.Persistence;
using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _684BakOMeter.Web.Data.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly AppDbContext _db;

    public PlayerRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Player>> GetAllAsync()
        => await _db.Players.OrderBy(p => p.Name).ToListAsync();

    public async Task<Player?> GetByIdAsync(int id)
        => await _db.Players.FindAsync(id);

    public async Task AddAsync(Player player)
    {
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Player player)
    {
        _db.Players.Update(player);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player is not null)
        {
            _db.Players.Remove(player);
            await _db.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<Player?> GetByNameAsync(string normalizedName)
        => await _db.Players
                    .FirstOrDefaultAsync(p => p.Name.ToLower() == normalizedName.ToLower());
}
