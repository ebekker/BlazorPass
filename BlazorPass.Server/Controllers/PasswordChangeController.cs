using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorPass.Shared;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Unosquare.PassCore.Common;

namespace BlazorPass.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[EnableCors]
    public class PasswordChangeController : ControllerBase
    {
        private IPasswordChangeProvider _passwordChanger;

        public PasswordChangeController(IPasswordChangeProvider passwordChanger)
        {
            _passwordChanger = passwordChanger;
        }

        [HttpGet]
        public object Get()
        {
            return new
            {
                Now = DateTime.Now.ToString(),
                Environment.MachineName,
                Environment.OSVersion,
                Environment.Is64BitOperatingSystem,
                Environment.Is64BitProcess,
                Environment.Version,
                Environment.UserName,
            };
        }

        // POST: api/PasswordChange
        [HttpPost]
        public PasswordChangeResponse Post([FromBody] PasswordChangeRequest requ)
        {
            try
            {
                var error = _passwordChanger.PerformPasswordChange(
                    requ.Username, requ.CurrentPassword, requ.NewPassword);

                if (error != null)
                {
                    return new PasswordChangeResponse
                    {
                        ErrorCode = (int)error.ErrorCode,
                        ErrorName = $"{error.ErrorCode}:{error.FieldName}",
                        ErrorDetail = error.Message,
                    };
                }

                return new PasswordChangeResponse();
            }
            catch (Exception ex)
            {
                return new PasswordChangeResponse
                {
                    ErrorCode = -1,
                    ErrorName = "EXCEPTION",
                    ErrorDetail = "Encountered unexpected exception: " + ex.Message,
                };
            }
        }
    }
}
