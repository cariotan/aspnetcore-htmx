public class EmailRequest
{
	public EmailAddress? From { get; set; } = new("noreply@ripplenetworks.com.au", "Ripple Networks");
	public EmailAddress? To { get; set; }
	public string? Subject { get; set; }
	public string? Body { get; set; }
}