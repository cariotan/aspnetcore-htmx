static partial class StaticMethods
{
	public static string SigningSecret => Environment.GetEnvironmentVariable("JwtSecret") ?? throw new Exception("Jwt signing secret not set");
}