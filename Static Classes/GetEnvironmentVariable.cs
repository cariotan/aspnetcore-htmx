static partial class StaticMethods
{
	public static string GetEnvironmentVariable(string key)
	{
		var value = Environment.GetEnvironmentVariable(key);
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		else
		{
			throw new KeyNotFoundException($"Please set {key} correctly in environment variables");
		}
	}
}