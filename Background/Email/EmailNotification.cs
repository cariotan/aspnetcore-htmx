public class EmailNotification : EmailRequest
{
	public EmailNotification(string subject, string body)
	{
		Subject = subject;
		Body = body;
		To = new SendGrid.Standard.EmailAddress("ctan@trucell.com.au", "Cario Tan");
	}
}