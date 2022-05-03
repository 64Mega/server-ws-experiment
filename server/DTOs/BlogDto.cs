public class BlogDto
{
	public Guid Id { get; set; }
	public string? Title { get; set; }
	public string? Body { get; set; }
	public UserDto? User { get; set; }
}