using System;

namespace ModMail.Utilities
{
    public static class Utils
    {
        public static string CXGetEnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable)
                   ?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User)
                   ?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine)
                   ?? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
        }
    }
}