static partial class StaticMethods
{
	public static	string GetDefaultArea(EndpointDataSource endpointDataSource)
		{
			var endpoint = endpointDataSource.Endpoints
				.OfType<RouteEndpoint>()
				.FirstOrDefault(e => e.Metadata.GetMetadata<IRouteNameMetadata>()?.RouteName == "default");

			if (endpoint?.RoutePattern.Defaults.TryGetValue("area", out var areaName) == true)
			{
				return areaName?.ToString() ?? "V1";
			}

			return "V1";
		}
}