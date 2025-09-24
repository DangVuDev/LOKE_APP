using AspNetCore.Identity.Mongo;  // Import MongoIdentityUser, MongoRole, MongoIdentityConfiguration
using Core.Model.Aplication;
using Core.Model.Base;
using Core.Model.Settings;
using Core.Repository;
using Core.Repository.Interfaces;
using Core.Service.DeployService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration
{
    public static class MongoConfig
    {
        //builder.Services.AddMongoService(
        //    new MongoDbModelMapping { DbSettings = dbSetting1, Models = new Type[] { typeof(User), typeof(Product)
        //    }
        //},
        //    new MongoDbModelMapping { DbSettings = dbSetting2, Models = new Type[] { typeof(Order), typeof(Invoice) } }
        //);
    public static IServiceCollection AddMongoService(
            this IServiceCollection services,
            params MongoDbModelMapping[] mappings)
            {
                if (mappings == null || mappings.Length == 0)
                    throw new ArgumentException("Phải truyền ít nhất một mapping database -> models");

                foreach (var mapping in mappings)
                {
                    var db = mapping.DbSettings ?? throw new ArgumentNullException(nameof(mapping.DbSettings));
                    var models = mapping.Models ?? Array.Empty<Type>();

                    foreach (var modelType in models)
                    {
                        if (!typeof(BaseModel).IsAssignableFrom(modelType))
                            throw new ArgumentException($"{modelType.Name} phải kế thừa BaseModel");

                        // Đăng ký repository
                        var repoType = typeof(IBaseRepository<>).MakeGenericType(modelType);
                        var implRepoType = typeof(MongoRepository<>).MakeGenericType(modelType);

                        services.AddSingleton(repoType, sp =>
                            Activator.CreateInstance(implRepoType, db, modelType.Name)!);

                        // Đăng ký BaseService<T> với interface
                        var interfaceType = typeof(Core.Service.Interfaces.IBaseService<>).MakeGenericType(modelType);
                        var implServiceType = typeof(Core.Service.DeployService.BaseService<>).MakeGenericType(modelType);

                        services.AddScoped(interfaceType, implServiceType);
                    }
            }

                return services;
            }
        }
        public class MongoDbModelMapping
        {
            public MongoDbSettings DbSettings { get; set; } = default!;
            public Type[] Models { get; set; } = Array.Empty<Type>();
        }
}
