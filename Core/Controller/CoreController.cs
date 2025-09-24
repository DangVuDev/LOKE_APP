using Core.Model.DTO.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Controller
{
    [ApiController]
    public abstract class CoreController : ControllerBase
    {
        [NonAction]
        public override OkObjectResult Ok(object? value)
        {
            return base.Ok(new BaseControllerResponse<object>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = value is string msg ? msg : "Request succeeded.",
                Data = value is string ? null : value
            });
        }

        [NonAction]
        public override BadRequestObjectResult BadRequest(object? value)
        {
            return base.BadRequest(new BaseControllerResponse<object>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = value is string msg ? msg : "Bad request.",
                Data = value is string ? null : value
            });
        }

        [NonAction]
        public override UnauthorizedObjectResult Unauthorized(object? value)
        {
            return base.Unauthorized(new BaseControllerResponse<object>
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = value is string msg ? msg : "Unauthorized access.",
                Data = value is string ? null : value
            });
        }

        [NonAction]
        public override NotFoundObjectResult NotFound(object? value)
        {
            return base.NotFound(new BaseControllerResponse<object>
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = value is string msg ? msg : "Resource not found.",
                Data = value is string ? null : value
            });
        }

        /// <summary>
        /// Custom response helper
        /// </summary>
        [NonAction]
        protected new ObjectResult Response<T>(BaseControllerResponse<T> response)
        {
            return StatusCode(response.StatusCode, response);
        }
    }
}
