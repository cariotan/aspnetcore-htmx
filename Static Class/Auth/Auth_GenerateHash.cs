using System.Security.Cryptography;
using System.Text;

static partial class StaticMethods
{
	public static string Auth_GenerateHash(string token)
	{
		using var sha = SHA256.Create();

		var bytes = Encoding.UTF8.GetBytes(token);
		var hash = sha.ComputeHash(bytes);

		return Convert.ToHexString(hash);
	}
}