using Core.Model.Base;
using Core.Model.DTO.Response;
using Core.Model.Settings;
using Core.Repository.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Repository
{
    public class MongoRepository<T> : IBaseRepository<T> where T : BaseModel
    {
        protected readonly IMongoCollection<T> _collection;

        public MongoRepository(MongoDbSettings settings, string collectionName)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var client = new MongoClient(settings.ConnectionString); // tạo client mới từ settings
            var db = client.GetDatabase(settings.DatabaseName);
            _collection = db.GetCollection<T>(collectionName);
        }

        public async Task<BaseServiceResponse<T>> CreateAsync(T entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _collection.InsertOneAsync(entity);
                return BaseServiceResponse<T>.Ok(entity, "Created successfully");
            }
            catch (Exception ex)
            {
                return BaseServiceResponse<T>.Fail($"Create failed: {ex.Message}");
            }
        }

        public async Task<BaseServiceResponse<bool>> DeleteAsync(string id)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(e => e.Id == id);
                if (result.DeletedCount > 0)
                    return BaseServiceResponse<bool>.Ok(true, "Deleted successfully");
                else
                    return BaseServiceResponse<bool>.Fail("Entity not found");
            }
            catch (Exception ex)
            {
                return BaseServiceResponse<bool>.Fail($"Delete failed: {ex.Message}");
            }
        }

        public async Task<BaseServiceResponse<List<T>>> GetAllAsync(int skip = 0, int limit = 100)
        {
            try
            {
                var list = await _collection.Find(_ => true).Skip(skip).Limit(limit).ToListAsync();
                return BaseServiceResponse<List<T>>.Ok(list);
            }
            catch (Exception ex)
            {
                return BaseServiceResponse<List<T>>.Fail($"Get all failed: {ex.Message}");
            }
        }

        public async Task<BaseServiceResponse<T?>> GetByIdAsync(string id)
        {
            try
            {
                var entity = await _collection.Find(e => e.Id == id).FirstOrDefaultAsync();
                if (entity == null)
                    return BaseServiceResponse<T?>.Fail("Entity not found");
                return BaseServiceResponse<T?>.Ok(entity);
            }
            catch (Exception ex)
            {
                return BaseServiceResponse<T?>.Fail($"Get by id failed: {ex.Message}");
            }
        }

        public async Task<BaseServiceResponse<bool>> UpdateAsync(string id, T entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                var result = await _collection.ReplaceOneAsync(e => e.Id == id, entity);
                if (result.ModifiedCount > 0)
                    return BaseServiceResponse<bool>.Ok(true, "Updated successfully");
                else
                    return BaseServiceResponse<bool>.Fail("Entity not found or no changes made");
            }
            catch (Exception ex)
            {
                return BaseServiceResponse<bool>.Fail($"Update failed: {ex.Message}");
            }
        }
        public async Task<BaseServiceResponse<List<T>>> GetByFilterAsync(
        Expression<Func<T, bool>> filter)
            {
                try
                {
                    // 1️⃣ Thực hiện truy vấn Mongo theo điều kiện filter
                    var entities = await _collection.Find(filter).ToListAsync();

                    // 2️⃣ Kiểm tra có kết quả không
                    if (entities == null || entities.Count == 0)
                        return BaseServiceResponse<List<T>>.Fail("No matching entities found.");

                    // 3️⃣ Trả kết quả thành công
                    return BaseServiceResponse<List<T>>.Ok(entities, "Query successful.");
                }
                catch (Exception ex)
                {
                    // 4️⃣ Bắt lỗi trong quá trình truy vấn
                    return BaseServiceResponse<List<T>>.Fail($"Get by filter failed: {ex.Message}");
                }
            }

        public async Task<BaseServiceResponse<T?>> GetOneByFilterAsync(
            Expression<Func<T, bool>> filter)
        {
            try
            {
                var entity = await _collection.Find(filter).FirstOrDefaultAsync();
                if (entity == null)
                    return BaseServiceResponse<T?>.Fail("Entity not found.");

                return BaseServiceResponse<T?>.Ok(entity, "Query successful.");
            }
            catch (Exception ex)
            {
                return BaseServiceResponse<T?>.Fail($"Get by filter failed: {ex.Message}");
            }
        }

    }
}
