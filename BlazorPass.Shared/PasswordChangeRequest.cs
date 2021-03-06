﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorPass.Shared
{
    public class PasswordChangeRequest
    {
        public string Username { get; set; }

        public string CurrentPassword { get; set; }

        public string PresetPayload { get; set; }

        public string NewPassword { get; set; }

        public string Mfa { get; set; }
    }
}
