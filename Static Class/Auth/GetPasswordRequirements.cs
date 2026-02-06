using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

static partial class StaticMethods
{
	public static IEnumerable<string> GetPasswordRequirements(IOptions<IdentityOptions> options)
	{
		var identityOptions = options.Value;

		var reqs = new List<string>();

		if (identityOptions.Password.RequireDigit)
		{
			reqs.Add("Must contain at least one digit (0-9)");
		}

		if (identityOptions.Password.RequireLowercase)
		{
			reqs.Add("Must contain at least one lowercase letter (a-z)");
		}

		if (identityOptions.Password.RequireUppercase)
		{
			reqs.Add("Must contain at least one uppercase letter (A-Z)");
		}

		if (identityOptions.Password.RequireNonAlphanumeric)
		{
			reqs.Add("Must contain at least one non-alphanumeric character (e.g. !, @, #, $)");
		}

		reqs.Add($"Must be at least {identityOptions.Password.RequiredLength} characters long");

		if (identityOptions.Password.RequiredUniqueChars > 1)
		{
			reqs.Add($"Must contain at least {identityOptions.Password.RequiredUniqueChars} unique characters");
		}

		return reqs;
	}
}