public class UnhandledError
{
	public Guid Id { get; set; }
	public string? Message { get; set; }
	public DateTime DateCreated { get; set; }
	public DateTime? DateUpdated { get; set; }
	public string? StackTrace { get; set; }
	public string? Source { get; set; }
	public string? Exception { get; set; }
	public string? AdditionalInformation { get; set; }
	public string? UserId { get; set; }
	public string? Handled { get; set; }
}