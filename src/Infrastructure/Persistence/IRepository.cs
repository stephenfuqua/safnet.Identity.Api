using System.Collections.Generic;
using System.Threading.Tasks;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public interface IRepository<T>
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task DeleteAsync(T entity);
    }
}