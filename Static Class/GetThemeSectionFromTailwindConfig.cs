using Jint;
using Jint.Native;
using Jint.Native.Json; // Added for the Serializer
using System.Text.RegularExpressions;

static partial class StaticMethods
{
	public static string GetThemeSection(string fileContent)
	{
		// Regex isolates the theme block to avoid the 'require' plugin errors
		string pattern = @"theme\s*:\s*\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}";
		var match = Regex.Match(fileContent, pattern, RegexOptions.Singleline);

		if (!match.Success)
		{
			return "{}";
		}

		try
		{
			var engine = new Engine();
			engine.Execute($"var x = {{ {match.Value} }};");

			var themeValue = engine.GetValue("x").AsObject().Get("theme");

			// Use the JsonSerializer directly to avoid the Engine.Json property error
			var serializer = new JsonSerializer(engine);
			
			// Parameters: (Value to stringify, Replacer (null), Space/Indent ("\t"))
			return serializer.Serialize(themeValue, JsValue.Undefined, "\t").ToString();
		}
		catch (System.Exception ex)
		{
			return $"// Error: {ex.Message}";
		}
	}
}