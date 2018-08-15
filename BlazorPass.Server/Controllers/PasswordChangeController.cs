using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorPass.Server.MFA;
using BlazorPass.Shared;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Unosquare.PassCore.Common;

namespace BlazorPass.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[EnableCors]
    public class PasswordChangeController : ControllerBase
    {
        private ILogger _logger;
        private IPasswordChangeProvider _passwordChanger;
        private MfaResolver _mfa;

        public PasswordChangeController(ILogger<PasswordChangeController> logger,
                IPasswordChangeProvider passwordChanger, MfaResolver mfa)
        {
            _logger = logger;
            _passwordChanger = passwordChanger;
            _mfa = mfa;
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
                var mfaVerify = _mfa.Verify(requ.Username, requ.Mfa);
                if (!mfaVerify.success)
                {
                    _logger.LogWarning("MFA failed to verify");
                    return new PasswordChangeResponse
                    {
                        ErrorCode = -1,
                        ErrorName = "MFA_FAIL",
                        ErrorDetail = mfaVerify.details,
                    };

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error trying to verify MFA");
                return new PasswordChangeResponse
                {
                    ErrorCode = -1,
                    ErrorName = "EXCEPTION",
                    ErrorDetail = "Encountered unexpected exception: " + ex.Message,
                };
            }

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
                _logger.LogError(ex, "Failed to change password");
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
