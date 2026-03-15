using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
	public DateTime DateCreated { get; set; }
	public required string DatabaseName { get; set; }
	public required string Area { get; set; }

	public ApplicationUser()
	{

	}

	public ApplicationUser(string userName, DateTime dateCreated)
		: base(userName)
	{
		Email = userName;
		DateCreated = dateCreated;
	}
}