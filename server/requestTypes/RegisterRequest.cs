using Newtonsoft.Json;
public class RegisterRequest
{
	[JsonProperty("name")]
	public string Name { get; set; } = "";

	[JsonProperty("email")]
	public string Email { get; set; } = "";

	[JsonProperty("password")]
	public string Password { get; set; } = "";
}