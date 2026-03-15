using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

static partial class StaticMethods
{
	public static async Task<Result<ApplicationUser>> Auth_CreateUser(
		string email,
		UserManager<ApplicationUser> userManager,
		string? password = null,
		string? area = null
	)
	{
		email = email.Trim();

		ApplicationUser newUser = new(email, DateTime.Now)
		{
			DatabaseName = "", // Will be set after the user is created.
			Area = area ?? "V1",
		};

		var result = password is not null ? await userManager.CreateAsync(newUser, password) : await userManager.CreateAsync(newUser);

		if (!result.Succeeded)
		{
			return new Utilities.Error(result.Errors.First().Description);
		}
		
		// Setup database name for dbUser and add claim.
		{
			newUser.DatabaseName = ConvertToSafeFileNameString(newUser.Email + "_" + newUser.Id);

			var claims = new List<Claim>
			{
				new("database_name", newUser.DatabaseName),
				new("area", newUser.Area)
			};
			await userManager.AddClaimsAsync(newUser, claims);

			await userManager.UpdateAsync(newUser);
			Js("database_name:" + " " + newUser.DatabaseName);
		}

		// Setup user role for dbUser.
		{
			await userManager.AddToRoleAsync(newUser, "User");
		}

		return newUser;
	}

	private static string ConvertToSafeFileNameString(string input)
	{
		var unsafeChars = new char[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
		foreach (var c in unsafeChars)
		{
			input = input.Replace(c.ToString(), "");
		}
		return input;
	}
}