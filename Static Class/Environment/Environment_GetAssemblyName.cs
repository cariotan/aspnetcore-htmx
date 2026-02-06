using System.Reflection;

static partial class StaticMethods
{
	public static string Environment_GetAssemblyName() => Assembly.GetExecutingAssembly().GetName().Name!; // If this is null, I'll fire myself.
}