using System;
using Serilog.Events;

namespace ModMail.Serilog.Sinks
{
    public static class FormatUtils
    {
        public static string GetFormattedTime(DateTimeOffset time)
        {
            return time.ToString("HH:mm:ss");
        }

        public static string GetFormattedLevel(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    return "VRB";
                case LogEventLevel.Debug:
                    return "DBG";
                case LogEventLevel.Information:
                    return "INF";
                case LogEventLevel.Warning:
                    return "WRN";
                case LogEventLevel.Error:
                    return "ERR";
                case LogEventLevel.Fatal:
                    return "FTL";
                default:
                    return "UKN";
            }
        }
    }
}