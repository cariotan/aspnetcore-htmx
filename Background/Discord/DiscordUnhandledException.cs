public class DiscordUnhandledException : DiscordMessage
{
	public DiscordUnhandledException(Exception exception)
	{
		Message = $"""
			An unhandled exception has occured for Halo Dashboard
			{exception}
			""";
	}

	public override string Key => "Halo";
}