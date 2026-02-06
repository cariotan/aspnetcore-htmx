using System.Security.Cryptography;

static partial class StaticMethods
{
	public static string Auth_GenerateRandomString(int size = 32)
	{
		var bytes = new byte[size];
		RandomNumberGenerator.Fill(bytes);

		return Convert.ToBase64String(bytes)
			.Replace("+", "-")
			.Replace("/", "_")
			.Replace("=", "");
	}
}