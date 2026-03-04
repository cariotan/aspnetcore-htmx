using System.Reflection;

static partial class StaticMethods
{
	public static string Environment_GetWorkingDirectory()
	{
		var workingDirectory = AppContext.BaseDirectory;
		Js("workingDirectory:" + " " + workingDirectory);
		return workingDirectory;
	}
}