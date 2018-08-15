using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPass.Server.MFA
{
    public interface IMfaProvider
    {
        void Init(Dictionary<string, string> options);

        (bool success, string details) Verify(string username, string code);
    }
}
