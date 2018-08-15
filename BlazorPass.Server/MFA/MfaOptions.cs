using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPass.Server.MFA
{
    public class MfaOptions
    {
        public string Provider { get; set; }

        public Dictionary<string, string> Settings { get; set; }
    }
}
