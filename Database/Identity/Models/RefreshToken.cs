public class RefreshToken
{
	public int Id { get; set; }
	public required string Hash256ShaToken { get; set; }
	public DateTime DateCreated { get; set; }
	public DateTime DateExpires { get; set; }
	public string? Purpose { get; set; }
	public string? RevokedReason { get; set; }
	
	public required string UserId { get; set; }
	public ApplicationUser? User { get; set; }
}