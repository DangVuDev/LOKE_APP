using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Response
{
    public class BaseServiceResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static BaseServiceResponse<T> Ok(T data, string message = "Success")
        {
            return new BaseServiceResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static BaseServiceResponse<T> Fail(string message)
        {
            return new BaseServiceResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default
            };
        }
    }
}

