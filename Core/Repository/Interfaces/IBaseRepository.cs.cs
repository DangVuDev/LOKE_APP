using Core.Model.Base;
using Core.Model.DTO.Response;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Repository.Interfaces
{
    public interface IBaseRepository<T> where T : BaseModel
    {
        Task<BaseServiceResponse<T>> CreateAsync(T entity);
        Task<BaseServiceResponse<T?>> GetByIdAsync(string id);
        Task<BaseServiceResponse<List<T>>> GetAllAsync(int skip = 0, int limit = 100);
        Task<BaseServiceResponse<bool>> UpdateAsync(string id, T entity);
        Task<BaseServiceResponse<bool>> DeleteAsync(string id);
        Task<BaseServiceResponse<List<T>>> GetByFilterAsync(
        Expression<Func<T, bool>> filter);
        Task<BaseServiceResponse<T?>> GetOneByFilterAsync(
        Expression<Func<T, bool>> filter);
    }
}