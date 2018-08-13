using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorPass.Shared
{
    public class PasswordChangeResponse
    {
        public int ErrorCode { get; set; }

        public string ErrorName { get; set; }

        public string ErrorDetail { get; set; }
    }
}
