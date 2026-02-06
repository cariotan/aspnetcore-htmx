using System;
using System.IO;

static partial class StaticMethods
{
	public static string Environment_GetDatabasePath()
	{
		string folderName = Environment_GetAssemblyName();

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