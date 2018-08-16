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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Unosquare.PassCore.Common;

namespace BlazorPass.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[EnableCors]
    public class PasswordChangeController : ControllerBase
    {
        private ILogger _logger;
        private PresetOptions _presetOptions;
        private IPasswordChangeProvider _passwordChanger;
        private MfaResolver _mfa;

        public PasswordChangeController(ILogger<PasswordChangeController> logger,
                IOptions<PresetOptions> presetOptions,
                IPasswordChangeProvider passwordChanger, MfaResolver mfa)
        {
            _logger = logger;
            _presetOptions = presetOptions.Value;
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
        public PasswordChangeResponse Post([FromBody]PasswordChangeRequest requ)
        {
            if (string.IsNullOrEmpty(requ.Username) || string.IsNullOrEmpty(requ.CurrentPassword))
            {
                if (string.IsNullOrEmpty(requ.PresetPayload))
                    throw new Exception("missing username and/or password");
                var preset = ExtractPresetPayload(requ.PresetPayload);
                requ.Username = preset.username;
                requ.CurrentPassword = preset.password;
            }

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

        [HttpGet("verifypreset")]
        public string VerifyPreset(string preset)
        {
            if (string.IsNullOrEmpty(_presetOptions?.PrivateKey))
            {
                using (var c = new CryptoHelper(null))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Private = c.ExportPrivate,
                        Public = c.ExportPublic,
                    });
                }
            }

            return ExtractPresetPayload(preset).username;
        }

        private (string nonce, string username, string password) ExtractPresetPayload(string presetPayload)
        {
            if (string.IsNullOrEmpty(_presetOptions?.PrivateKey))
                throw new Exception("not setup for preset payload support");

            using (var crypto = new CryptoHelper(_presetOptions.PrivateKey))
            {
                var clear = crypto.Decrypt(presetPayload);
                var parts = clear.Split("\n");

                // Should be formatted as:
                //    guidNonce[nl]username[nl]password
                if (parts.Length != 3)
                    throw new Exception("malformed payload");

                if (!Guid.TryParse(parts[0], out var nonce) || nonce.ToString() != parts[0])
                    throw new Exception("invalid nonce");

                // Assume username & password are correct
                return (parts[0], parts[1], parts[2]);
            }
        }
    }
}
