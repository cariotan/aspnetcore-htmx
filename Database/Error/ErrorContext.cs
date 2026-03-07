using Microsoft.EntityFrameworkCore;

/*
dotnet ef migrations add InitialMigration -c ErrorContext -o Migrations/ErrorDatabase
dotnet ef database update -c ErrorContext
*/

namespace ErrorDatabase;

public class ErrorContext : DbContext
{
		private string connectionString = $"Data Source={Environment_GetDatabasePath()}\\ErrorContext.db";
		private static SqlitePragmaInterceptor sqlitePragmaInterceptor = new();

		public DbSet<Error> UnhandledErrors { get; set; } = null!;

		public ErrorContext()
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.AddInterceptors(sqlitePragmaInterceptor).UseSqlite(connectionString);
		}
}