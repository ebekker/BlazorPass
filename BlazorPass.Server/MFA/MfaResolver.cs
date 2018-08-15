using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPass.Server.MFA
{
    public class MfaResolver
    {
        private ILogger _logger;
        private MfaOptions _options;
        private IMfaProvider _provider;

        public MfaResolver(ILogger<MfaResolver> logger,
            ILogger<IMfaProvider> providerLogger,
            IOptions<MfaOptions> options)
        {
            _logger = logger;
            _options = options?.Value;
            if (string.IsNullOrEmpty(_options?.Provider))
                return;

            if (_options.Provider == "DUO")
                _provider = new DuoMfaProvider(providerLogger);
            else
                throw new Exception("unknown or unsupported MFA provider: " + _options.Provider);

            _provider.Init(_options.Settings);
        }

        public (bool success, string details) Verify(string username, string code)
        {
            return _provider == null ? (true, null) : _provider.Verify(username, code);
        }
    }
}
