using System;
using System.IO;

static partial class StaticMethods
{
    public static readonly string DatabasePath;

    static StaticMethods()
    {
        if (OperatingSystem.IsWindows())
        {
            DatabasePath = @"C:\Database\ccTimerNet";
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            DatabasePath = Path.Combine(home, "Database", "ccTimerNet");
        }
    }
}
