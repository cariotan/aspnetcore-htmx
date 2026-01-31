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

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		#warning Change these if you want.
		const string SUPERADMIN_ROLE_ID = "19F014BA-8247-4992-B8AD-9DC90B5685CB";
		const string ADMIN_ROLE_ID = "A72D6D34-3B0E-446F-86AF-483D56FD7210";
		const string USER_ID = "B91F285F-4C1F-557B-97BC-594E67BE8321";

		builder.Entity<ApplicationRole>().HasData(
			new ApplicationRole
			{
				Id = SUPERADMIN_ROLE_ID,
				Name = "SuperAdmin",
				NormalizedName = "SUPERADMIN"
			},
			new ApplicationRole
			{
				Id = ADMIN_ROLE_ID,
				Name = "Admin",
				NormalizedName = "ADMIN"
			},
			new ApplicationRole
			{
				Id = USER_ID,
				Name = "User",
				NormalizedName = "USER"
			}
		);
	}
}

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