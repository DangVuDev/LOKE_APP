using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper
{
    public static class MapperHelper
    {
        public static IMapper Mapper { get; private set; } = null!;
        public static readonly Dictionary<Type, Type> TypePairs = new();

        public static void Initialize(IMapper mapper, IEnumerable<(Type model, Type dto)> pairs)
        {
            Mapper = mapper;

            foreach (var (model, dto) in pairs)
            {
                // Lưu 2 chiều: model -> dto và dto -> model
                if (!TypePairs.ContainsKey(model))
                    TypePairs[model] = dto;

                if (!TypePairs.ContainsKey(dto))
                    TypePairs[dto] = model;
            }
        }

        public static Type? GetDestination(Type sourceType)
        {
            return TypePairs.TryGetValue(sourceType, out var dest) ? dest : null;
        }
    }

}
