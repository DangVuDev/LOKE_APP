using AutoMapper;
using Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Extention
{
    public static class MappingExtensions
    {
        /// <summary>
        /// Convert model sang DTO (auto lookup từ dictionary)
        /// </summary>
        public static object ToDto<TSource>(this TSource source)
        {
            var mapper = MapperHelper.Mapper;
            var destinationType = MapperHelper.GetDestination(typeof(TSource));

            if (destinationType == null)
                throw new InvalidOperationException($"No mapping found for {typeof(TSource).Name}");

            return mapper.Map(source, typeof(TSource), destinationType);
        }

        /// <summary>
        /// Convert list model sang list DTO
        /// </summary>
        public static IEnumerable<object> ToDtoList<TSource>(this IEnumerable<TSource> source)
        {
            var mapper = MapperHelper.Mapper;
            var destinationType = MapperHelper.GetDestination(typeof(TSource));

            if (destinationType == null)
                throw new InvalidOperationException($"No mapping found for {typeof(TSource).Name}");

            return source.Select(item => mapper.Map(item, typeof(TSource), destinationType));
        }

        /// <summary>
        /// Convert DTO sang Model
        /// </summary>
        public static object ToModel<TDto>(this TDto dto)
        {
            var mapper = MapperHelper.Mapper;
            var destinationType = MapperHelper.GetDestination(typeof(TDto));

            if (destinationType == null)
                throw new InvalidOperationException($"No mapping found for {typeof(TDto).Name}");

            return mapper.Map(dto, typeof(TDto), destinationType);
        }

        /// <summary>
        /// Convert list DTO sang list Model
        /// </summary>
        public static IEnumerable<object> ToModelList<TDto>(this IEnumerable<TDto> dtos)
        {
            var mapper = MapperHelper.Mapper;
            var destinationType = MapperHelper.GetDestination(typeof(TDto));

            if (destinationType == null)
                throw new InvalidOperationException($"No mapping found for {typeof(TDto).Name}");

            return dtos.Select(item => mapper.Map(item, typeof(TDto), destinationType));
        }

        /// <summary>
        /// Convert sang type cụ thể (type-safe hơn)
        /// </summary>
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return MapperHelper.Mapper.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Convert list sang type cụ thể (type-safe hơn)
        /// </summary>
        public static IEnumerable<TDestination> MapToList<TSource, TDestination>(this IEnumerable<TSource> source)
        {
            return MapperHelper.Mapper.Map<IEnumerable<TSource>, IEnumerable<TDestination>>(source);
        }

        /// <summary>
        /// Update object hiện tại từ DTO (giúp patch update entity)
        /// </summary>
        public static TDestination UpdateFrom<TDestination, TSource>(this TDestination destination, TSource source)
        {
            return MapperHelper.Mapper.Map(source, destination);
        }
    }
}
