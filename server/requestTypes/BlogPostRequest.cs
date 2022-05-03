using Newtonsoft.Json;

public class BlogPostRequest {
	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("body")]
	public string Body { get; set; }
}