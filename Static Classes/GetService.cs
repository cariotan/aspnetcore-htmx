static partial class StaticMethods
{
	public static T GetService<T>(this IServiceScope serviceScope)
	{
		var service = serviceScope.ServiceProvider.GetRequiredService<T>();
		return service;
	}
}