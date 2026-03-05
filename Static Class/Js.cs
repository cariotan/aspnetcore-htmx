using System.Text.Encodings.Web;
using System.Text.Json;

static partial class StaticMethods
{
	static JsonSerializerOptions jsonSerializerOptions = new()
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static void Js(object @object)
	{
		if (@object is string || @object is int || @object is char || @object is bool || @object is double || @object is float || @object is decimal || @object is long)
		{
			Console.WriteLine(@object);
		}

		Console.WriteLine(JsonSerializer.Serialize(@object, jsonSerializerOptions));
	}
}