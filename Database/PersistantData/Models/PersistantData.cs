public class PersistantData
{
	[Key]
	public required string SessionId { get; set; }
	public required DateTime DateCreated { get; set; }
	public required string Type { get; set; }
	public required string DataJson { get; set; }
}