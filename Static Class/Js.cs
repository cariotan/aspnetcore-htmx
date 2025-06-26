using System.Text.Encodings.Web;
using System.Text.Json;

static partial class StaticMethods
{
	public static void Js(object @object)
	{
		Console.WriteLine(JsonSerializer.Serialize(@object, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}));
	}
}