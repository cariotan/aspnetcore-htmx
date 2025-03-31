public class DiscordMessage
{
	public string Message { get; set; } = "";
	public DateTime SentDate { get; set; } = DateTime.Now;

	/// <summary>
	/// The key for the webhook from appsettings.json
	/// </summary>
	public string Key { get; set; } = "Default";
}