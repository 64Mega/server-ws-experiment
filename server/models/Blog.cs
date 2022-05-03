using System.Text.Json.Serialization;

public class Blog
{
	public Guid BlogId { get; set; }
	public string? Title { get; set; }
	public string? Body { get; set; }

	public Guid UserId { get; set; }
	[JsonIgnore]
	public User User { get; set; }
}