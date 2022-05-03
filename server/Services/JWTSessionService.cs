// Simplistic in-memory user session tracking
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public class JWTSessionService
{
	private List<JWT> _tokens = new List<JWT>();
	private ILogger _logger;
	private IConfiguration _configuration;

	public JWTSessionService(ILogger<JWTSessionService> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	public JWT GenerateJWT(User user)
	{
		// Could check here if user is already in a session to log out of all other sessions.
		// Leaving it like this allows for multi-device/browser login.

		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var newJWTId = Guid.NewGuid();
		var claims = new[] {
			new Claim(JwtRegisteredClaimNames.Sub, user.Name),
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
			new Claim(JwtRegisteredClaimNames.Jti, newJWTId.ToString())
		};

		var expires = DateTime.Now.AddMinutes(120);

		var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
			_configuration["Jwt:Issuer"],
			claims,
			expires: expires,
			signingCredentials: credentials
		);

		var signedToken = new JwtSecurityTokenHandler().WriteToken(token);

		var jwt = new JWT(newJWTId, signedToken, expires, user.Name, user.Email);
		_tokens.Add(jwt);

		return jwt;
	}

	public User? getSessionUser(string bearerToken, BloggingContext db)
	{
		if (IsAuthorized(bearerToken))
		{
			var token = readJwtSecurityToken(bearerToken);
			var email = token.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

			if (email != null)
			{
				return db.Users.FirstOrDefault(user => user.Email == email);
			}
		}

		return null;
	}

	public bool IsAuthorized(string? rawToken)
	{
		if (rawToken == null) return false;
		var tokenResult = readJwtSecurityToken(rawToken);
		var existing = readToken(rawToken);
		if (existing != null && tokenResult != null)
		{
			var claimedName = tokenResult.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;
			var claimedEmail = tokenResult.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

			if (claimedName == existing._userName && claimedEmail == existing._userEmail)
			{
				return existing.isValid();
			}
			else
			{
				return false;
			}
		}
		return false;
	}


	private JwtSecurityToken readJwtSecurityToken(string rawToken)
	{
		var tokenParts = rawToken.Split(' ');
		if (tokenParts.Length != 2 || tokenParts[0] != "Bearer") return null;

		var token = tokenParts[1];

		var handler = new JwtSecurityTokenHandler();
		return handler.ReadJwtToken(token);
	}
	private JWT? readToken(string rawToken)
	{
		var tokenResult = readJwtSecurityToken(rawToken);
		var tokenId = tokenResult.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;

		if (tokenId != null)
		{
			// Find token in existing session store.
			var existing = _tokens.FirstOrDefault(token => token._id == Guid.Parse(tokenId));
			return existing;
		}

		return null;
	}
}