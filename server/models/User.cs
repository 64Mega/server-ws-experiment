using System.Text.Json.Serialization;

public class User
{
	public Guid UserId { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
	public string PasswordHash { get; set; }

	public byte[] UserSalt { get; set; }

	[JsonIgnore]
	public List<Blog> Blogs { get; } = new();
}