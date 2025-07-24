using System;
using System.IO;

static partial class StaticMethods
{
	public static string GetDatabasePath()
    {
		#warning Set folder name for database
		string folderName = "";

        if (OperatingSystem.IsWindows())
        {
            return $"""C:\Database\{folderName}""";
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Database", folderName);
        }
    }
}