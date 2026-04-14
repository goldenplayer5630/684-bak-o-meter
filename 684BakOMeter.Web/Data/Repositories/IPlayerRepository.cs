using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Data.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player?> GetByIdAsync(int id);
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
    Task DeleteAsync(int id);
    Task<Player?> GetByNameAsync(string normalizedName);
}
