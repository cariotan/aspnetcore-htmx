using System.Text.Json;

static partial class StaticMethods
{
	public static string Js(object @object)
	{
		return JsonSerializer.Serialize(@object);
	}
}