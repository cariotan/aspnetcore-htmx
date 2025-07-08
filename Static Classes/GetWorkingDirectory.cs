using System.Reflection;

static partial class StaticMethods
{
	public static string GetWorkingDirectory()
	{
		return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
	}
}