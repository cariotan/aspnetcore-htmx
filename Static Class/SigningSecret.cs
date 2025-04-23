static partial class StaticMethods
{
	public static string SigningSecret => Environment.GetEnvironmentVariable("JwtSecret", EnvironmentVariableTarget.Machine) ?? throw new Exception("Jwt signing secret not set");
}