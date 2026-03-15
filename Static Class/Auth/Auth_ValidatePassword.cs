using Microsoft.AspNetCore.Identity;

static partial class StaticMethods
{
	public static async Task<bool> Auth_ValidatePassword(
		string password,
		IPasswordValidator<ApplicationUser> passwordValidator,
		UserManager<ApplicationUser> userManager
	)
	{
		ApplicationUser dummyUser = new("dummy@example.com", DateTime.Now)
		{
			DatabaseName = "dummy",
			Area = "",
		};

		return (await passwordValidator.ValidateAsync(userManager, dummyUser, password)).Succeeded;
	}
}