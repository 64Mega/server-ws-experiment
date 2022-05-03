using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins("https://localhost:1234", "https://127.0.0.1:1234")
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Issuer"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
		};
	});
builder.Logging.AddConsole();
builder.Services.AddSingleton<JWTSessionService>();
builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddHostedService<WebSocketService>(p => p.GetRequiredService<WebSocketService>());
builder.Services.AddDbContext<BloggingContext>();

var app = builder.Build();
app.UseAuthentication();
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseWebSockets();

app.Use(async (context, next) =>
{
	if (context.Request.Path == "/ws")
	{
		using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
		var socketFinishedTcs = new TaskCompletionSource<object>();

		WebSocketService.RegisterClient(new Client(webSocket, socketFinishedTcs));

		await socketFinishedTcs.Task;
	}
	else
	{
		await next(context);
	}
});

app.MapGet("/blogs", async ([FromQuery] int? page, [FromHeader(Name = "Authorization")] string? bearerToken, ILogger<WebApplication> logger, BloggingContext db, JWTSessionService jwtService) =>
{
	// Send result
	//int offset = page != null ? (int)page : 0;
	//return await db.Blogs.Take(new Range(offset * 20, (offset * 20) + 20)).ToListAsync();
	//var blogs = db.Blogs.Take(20).Include("User").ToListAsync();

	var blogs = await db.Blogs.OrderByDescending(b => b.BlogId).Include(b => b.User).Select(blog => new BlogDto
	{
		Id = blog.BlogId,
		Title = blog.Title,
		Body = blog.Body,
		User = new UserDto
		{
			Name = blog.User.Name,
			Id = blog.User.UserId
		}
	}).ToListAsync();

	return Results.Ok(blogs);
});

app.MapGet("/users", async ([FromHeader(Name = "Authorization")] string? bearerToken, BloggingContext db, JWTSessionService jwtService) =>
{
	if (jwtService.IsAuthorized(bearerToken))
	{
		return Results.Ok(await db.Users.Take(50).ToListAsync());
	}
	else
	{
		return Results.Unauthorized();
	}
});

app.MapPost("/user/login", async ([FromBody] LoginRequest req, BloggingContext db, JWTSessionService jwtService) =>
{
	var existingUser = await db.Users.Where(user => user.Email == req.Email).SingleOrDefaultAsync();
	if (existingUser != null)
	{
		byte[] salt = existingUser.UserSalt;

		string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: req.Password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 100000,
			numBytesRequested: 256 / 8
		));

		if (hash == existingUser.PasswordHash)
		{
			var token = jwtService.GenerateJWT(existingUser);
			return Results.Ok(new
			{
				status = 200,
				message = "Login Successful!",
				token = token._signedToken
			});
		}
		else
		{
			return Results.BadRequest();
		}
	}
	else
	{
		return Results.BadRequest();
	}
});

app.MapPost("/user/register", async ([FromBody] RegisterRequest req, BloggingContext db) =>
{
	var existingUser = await db.Users.Where(user => user.Email == req.Email).SingleOrDefaultAsync();
	if (existingUser == null)
	{
		byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

		string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: req.Password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 100000,
			numBytesRequested: 256 / 8
		));

		var newUser = new User
		{
			Name = req.Name,
			Email = req.Email,
			PasswordHash = hash,
			UserSalt = salt
		};

		await db.Users.AddAsync(newUser);
		await db.SaveChangesAsync();
		return new OkObjectResult(new
		{
			status = 200,
			message = "User registered"
		});
	}
	else
	{
		return new OkObjectResult(new
		{
			error = "User email already exists in system.",
			status = 403
		});
	}
});

app.MapPost("/blog/post", async ([FromBody] BlogPostRequest req, [FromHeader(Name = "Authorization")] string bearerToken, BloggingContext db, JWTSessionService jwtService, WebSocketService socketService) =>
{
	if (jwtService.IsAuthorized(bearerToken))
	{
		var user = jwtService.getSessionUser(bearerToken, db);
		if (user != null)
		{
			var blog = new Blog
			{
				Body = req.Body,
				Title = req.Title,
				UserId = user.UserId
			};

			await db.Blogs.AddAsync(blog);
			await db.SaveChangesAsync();
			socketService.BroadcastMessage(new
			{
				action = "BLOGPOST",
			});
			return Results.Ok();
		}
		else
		{
			return Results.BadRequest();
		}
	}
	else
	{
		return Results.Unauthorized();
	}
});

app.Run();
