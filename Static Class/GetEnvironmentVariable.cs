static partial class StaticMethods
{
	public readonly static Dictionary<string, string?> EnvironmentVariables = new()
	{
		["Test"] = Environment.GetEnvironmentVariable("Test"),
		["Exception"] = Environment.GetEnvironmentVariable("Exception"),
		["JwtSecret"] = Environment.GetEnvironmentVariable("JwtSecret"),
		["JwtSecretAdmin"] = Environment.GetEnvironmentVariable("JwtSecretAdmin"),
		["SENDGRID_API_KEY"] = Environment.GetEnvironmentVariable("SENDGRID_API_KEY"),
		["github-client-id"] = Environment.GetEnvironmentVariable("github-client-id"),
		["github-client-secret"] = Environment.GetEnvironmentVariable("github-client-secret"),
		["google-client-id"] = Environment.GetEnvironmentVariable("google-client-id"),
		["google-client-secret"] = Environment.GetEnvironmentVariable("google-client-secret"),
	};

	public static string? GetEnvironmentVariable(string key)
	{
		return EnvironmentVariables[key];
	}
}