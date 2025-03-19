public class EmailRequest
{
	// public SendGrid.Standard.EmailAddress? From { get; set; } = new("noreply@ripplenetworks.com.au", "Ripple Networks");
	// public SendGrid.Standard.EmailAddress? To { get; set; }
	public string? Subject { get; set; }
	public string? Body { get; set; }
}