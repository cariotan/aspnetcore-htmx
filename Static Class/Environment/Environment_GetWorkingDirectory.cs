using System.Reflection;

static partial class StaticMethods
{
	public static string Environment_GetWorkingDirectory()
	{
		return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
	}
}