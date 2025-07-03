using JasperFx;
using Wolverine.Marten;

static partial class StaticMethods
{
	public static void SetupWolverineAndMarten(this WebApplicationBuilder builder)
	{
		builder.UseWolverine(x =>
		{
			x.Policies.AutoApplyTransactions();
		});

		builder.Services.AddMarten(x =>
		{
			x.Connection("Host=localhost;Port=5432;Database=;Username=;Password=");
			x.UseSystemTextJsonForSerialization();
			if (builder.Environment.IsDevelopment())
			{
				x.AutoCreateSchemaObjects = AutoCreate.All;
				x.DisableNpgsqlLogging = true;
			}
		}).IntegrateWithWolverine();
	}
}