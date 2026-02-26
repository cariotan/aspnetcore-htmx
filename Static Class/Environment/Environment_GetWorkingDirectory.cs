using System.Reflection;

static partial class StaticMethods
{
	public static string GetWorkingDirectory()
	{
		var workingDirectory = AppContext.BaseDirectory;
		Js("workingDirectory:" + " " + workingDirectory);
		return workingDirectory;
	}
}