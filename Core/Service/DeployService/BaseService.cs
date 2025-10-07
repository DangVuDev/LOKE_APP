using Core.Model.Base;
using Core.Model.DTO.Response;
using Core.Repository.Interfaces;
using Core.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Service.DeployService
{
    public class BaseService<T> : IBaseService<T> where T : BaseModel
    {
        protected readonly IBaseRepository<T> _repository;

        public BaseService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<BaseServiceResponse<T>> CreateAsync(T entity) => await _repository.CreateAsync(entity);
        public async Task<BaseServiceResponse<bool>> DeleteAsync(string id) => await _repository.DeleteAsync(id);
        public async Task<BaseServiceResponse<List<T>>> GetAllAsync(int skip = 0, int limit = 100) => await _repository.GetAllAsync(skip, limit);
        public async Task<BaseServiceResponse<T?>> GetByIdAsync(string id) => await _repository.GetByIdAsync(id);
        public async Task<BaseServiceResponse<bool>> UpdateAsync(string id, T entity) => await _repository.UpdateAsync(id, entity);
        public async Task<BaseServiceResponse<List<T>>> GetByFilterAsync(Expression<Func<T, bool>> filter) => await _repository.GetByFilterAsync(filter);
        public async Task<BaseServiceResponse<T?>> GetOneByFilterAsync(Expression<Func<T, bool>> filter) => await _repository.GetOneByFilterAsync(filter);
    }
}
