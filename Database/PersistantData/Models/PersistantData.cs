public class PersistantData
{
	public Guid Id { get; set; }
	public required string SessionId { get; set; }
	public required DateTime DateCreated { get; set; }
	public required string Type { get; set; }
	public required string DataJson { get; set; }
}