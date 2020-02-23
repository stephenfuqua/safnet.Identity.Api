using System.Collections.Generic;
using System.Threading.Tasks;

namespace safnet.Identity.Api.Infrastructure.Persistence
{
    public interface IRepository<T>
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetAsync(int id);
        Task<int> CreateAsync(T model);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(int id);
        Task<int> DeleteAsync(T entity);
    }
}