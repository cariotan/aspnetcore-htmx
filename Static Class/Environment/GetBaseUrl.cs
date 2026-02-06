static partial class StaticMethods
{
	public static string GetBaseUrl(HttpRequest request)
	{
		var baseUrl = $"{request.Scheme}://{request.Host}";
		return baseUrl;
	}
}