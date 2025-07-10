using System.Text.Encodings.Web;
using System.Text.Json;

static partial class StaticMethods
{
	private static JsonSerializerOptions options = new()
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static void Js(object @object)
	{
		Console.WriteLine(JsonSerializer.Serialize(@object, options));
	}
}