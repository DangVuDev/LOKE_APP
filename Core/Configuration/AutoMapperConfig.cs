using AutoMapper;
using Core.Helper;
using Core.Model.Aplication;
using Core.Model.DTO.Model;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration
{
    public static class AutoMapperConfig
    {
        public static IServiceCollection AutoMap(this IServiceCollection services, params (Type model, Type dto)[] pairs)
        {
            // Replace this line:
            // var config = new MapperConfiguration(cfg =>
            // With the following:

            var config = new MapperConfiguration(cfg =>
            {
                foreach (var (model, dto) in pairs)
                {
                    // CreateMap<Model, DTO>().ReverseMap();
                    var createMapMethod = typeof(IProfileExpression)
                        .GetMethods()
                        .First(m => m.Name == "CreateMap" && m.IsGenericMethod && m.GetGenericArguments().Length == 2)
                        .MakeGenericMethod(model, dto);

                    var mapExpr = createMapMethod.Invoke(cfg, null);

                    // ReverseMap
                    var mapType = typeof(IMappingExpression<,>).MakeGenericType(model, dto);
                    var reverseMapMethod = mapType.GetMethod("ReverseMap");
                    if (reverseMapMethod != null && mapExpr != null)
                    {
                        reverseMapMethod.Invoke(mapExpr, null);
                    }
                }
            });

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            MapperHelper.Initialize(mapper, pairs);

            return services;
        }
    }
}
