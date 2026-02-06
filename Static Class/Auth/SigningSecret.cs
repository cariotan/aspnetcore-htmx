static partial class StaticMethods
{
	public static string? SigningSecret => GetEnvironmentVariable("JwtSecret");
}