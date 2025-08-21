using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/*
dotnet ef migrations add InitialMigration -c IdentityContext
dotnet ef database update -c IdentityContext
*/

public class IdentityContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
	private string connectionString = $"Data Source={GetDatabasePath()}\\IdentityContext.db";

	public DbSet<RefreshToken> RefreshTokens { get; set; }

	public IdentityContext()
	{

	}

	public IdentityContext(string connectionString)
	{
		this.connectionString = connectionString;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlite(connectionString);
	}
}

public class ApplicationUser : IdentityUser
{
	public string? GoogleId { get; set; }

	public ApplicationUser()
	{

	}

	public ApplicationUser(string userName)
		: base(userName)
	{
		Email = userName;
	}
}

public class ApplicationRole : IdentityRole
{
	public ApplicationRole()
	{

	}

	public ApplicationRole(string role)
		: base(role)
	{

	}
}

public class RefreshToken
{
	public int Id { get; set; }
	public required string Hash256ShaToken { get; set; }
	public DateTime DateCreated { get; set; }
	public DateTime DateExpires { get; set; }
	public string? Purpose { get; set; }
	public string? RevokedReason { get; set; }
	
	public required string UserId { get; set; }
	public ApplicationUser? User { get; set; }
}