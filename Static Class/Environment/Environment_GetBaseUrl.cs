static partial class StaticMethods
{
	public static string Environment_GetBaseUrl(HttpRequest request)
	{
		var baseUrl = $"{request.Scheme}://{request.Host}";
		return baseUrl;
	}
}