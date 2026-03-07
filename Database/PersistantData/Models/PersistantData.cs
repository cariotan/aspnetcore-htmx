public class PersistantData
{
	public Guid Id { get; set; }
	public required string Key { get; set; }
	public required DateTime DateCreated { get; set; }
	public required string Type { get; set; }
	public required string DataJson { get; set; }
}