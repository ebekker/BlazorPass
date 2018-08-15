using Duo;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorPass.Server.MFA
{
    public class DuoMfaProvider : IMfaProvider
    {
        private ILogger _logger;
        private string _iKey;
        private string _sKey;
        private string _host;

        public DuoMfaProvider(ILogger logger)
        {
            _logger = logger;
        }

        public void Init(Dictionary<string, string> options)
        {
            if (!options.TryGetValue("ikey", out _iKey))
                throw new Exception("missing [ikey]");
            if (!options.TryGetValue("skey", out _sKey))
                throw new Exception("missing [skey]");
            if (!options.TryGetValue("host", out _host))
                throw new Exception("missing [host]");

            var skey = string.IsNullOrEmpty(_sKey) ? "" : "*** (" + _sKey.Length + ")";

            _logger.LogInformation("DUO MFA INIT:");
            _logger.LogInformation($"  * iKey [{_iKey}]");
            _logger.LogInformation($"  * sKey [{skey}]");
            _logger.LogInformation($"  * iKey [{_host}]");
        }

        public (bool success, string details) Verify(string username, string code)
        {
            var duoParams = new Dictionary<string, string>
            {
                ["username"] = username,
                ["factor"] = "passcode",
            };

            if (code == "push")
            {
                duoParams["factor"] = "push";
                duoParams["device"] = "auto";
            }
            else if (code == "phone")
            {
                duoParams["factor"] = "phone";
                duoParams["device"] = "auto";
            }
            else
            {
                duoParams["passcode"] = code;
            };

            var client = new DuoApi(_iKey, _sKey, _host);

            HttpStatusCode statusCode;
            string res = client.ApiCall("POST", "/auth/v2/auth", duoParams, 0, DateTime.UtcNow, out statusCode);
            var resp = client.JSONDecodeCall<Dictionary<string, object>>(res, statusCode);

            _logger.LogInformation("DUO MFA RESP: " + res);
            _logger.LogInformation("DUO MFA RESP: " + JsonConvert.SerializeObject(resp));

            resp.TryGetValue("result", out var result);
            resp.TryGetValue("status_msg", out var statusMsg);

            return ((((result as string) ?? "") == "allow"),
                statusMsg as string);
        }
    }
}
