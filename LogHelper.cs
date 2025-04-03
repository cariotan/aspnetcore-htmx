public static partial class LogHelper
{
	[LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "{Method} {Path}")]
	public static partial void Endpoint(this ILogger logger, Method method, string path);
}

public enum Method
{
	Get, Post, Delete, Put, Patch
}