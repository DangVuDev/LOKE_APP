using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.DTO.Response
{
    public class BaseControllerResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static BaseControllerResponse<T> Ok(T data, string message = "Success")
        {
            return new BaseControllerResponse<T>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = message,
                Data = data
            };
        }

        public static BaseControllerResponse<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new BaseControllerResponse<T>
            {
                StatusCode = (int)statusCode,
                Message = message,
                Data = default
            };
        }
    }
}
