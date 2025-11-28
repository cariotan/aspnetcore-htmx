using Newtonsoft.Json;

static partial class StaticMethods
{
	public static void Js(object @object)
	{
		Console.WriteLine(JsonConvert.SerializeObject(@object, Formatting.Indented));
	}
}