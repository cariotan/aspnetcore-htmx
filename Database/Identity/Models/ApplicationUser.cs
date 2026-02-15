using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
	public ApplicationUser()
	{

	}

	public ApplicationUser(string userName)
		: base(userName)
	{
		Email = userName;
	}
}