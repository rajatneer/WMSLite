using WMSLite.Models;

namespace WMSLite.Repositories;

public interface IJsonRepository<T> where T : class, IEntity
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task InsertAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}
