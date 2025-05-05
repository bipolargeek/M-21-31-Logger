using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace M_21_31.Logger
{
    public static class Log4Net_LogManager
    {

        public static ILog GetM_21_31_Logger(Type type)
        {
            return new Log4Net_Logger(type);
        }

    }
}
