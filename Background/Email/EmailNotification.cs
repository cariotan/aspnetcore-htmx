public class EmailNotification : EmailRequest
{
	public EmailNotification(string subject, string body)
	{
		Subject = subject;
		Body = body;
		To = new EmailAddress("ctan@trucell.com.au", "Cario Tan");
	}
}

public record EmailAddress(string Email, string Name);