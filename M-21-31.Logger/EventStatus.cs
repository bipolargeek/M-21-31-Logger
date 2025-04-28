using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace M_21_31.Logger
{
    public enum EventStatus
    {
        [Description("Success")]
        Success = 200,
        [Description("Fail")]
        Fail = 500
    }
}
