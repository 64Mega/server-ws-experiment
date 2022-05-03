using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class Api
{
	public static void RegisterApiRoutes(this WebApplication app)
	{
	}

	private static async Task<IResult> GetBlogs([FromQuery] int? page, [FromHeader(Name = "Authorization")] string? bearerToken, ILogger<WebApplication> logger, BloggingContext db, JWTSessionService jwtService)
	{
		if (bearerToken != null)
		{
			var tokenParts = bearerToken.Split(' ');
			if (tokenParts.Length == 2 && tokenParts[0] == "Bearer")
			{
				// Find user for this
				if (jwtService.IsAuthorized(tokenParts[1]))
				{
					// Send result
					int offset = page != null ? (int)page : 0;
					//return await db.Blogs.Take(new Range(offset * 20, (offset * 20) + 20)).ToListAsync();
					var blogs = await db.Blogs.Take(20).ToListAsync();
					return Results.Ok(blogs);
				}
			}
		}
		return Results.Problem("Unauthorized");
	}
}