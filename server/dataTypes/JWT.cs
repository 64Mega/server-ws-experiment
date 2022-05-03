public class JWT
{

	public DateTime _expires { get; set; } = new DateTime();
	public string _signedToken { get; set; }
	public string _userName { get; set; }
	public string _userEmail { get; set; }
	public Guid _id { get; set; }
	public JWT(Guid id, string signedToken, DateTime expires, string userName, string userEmail)
	{
		this._id = id;
		this._signedToken = signedToken;
		this._expires = expires;
		this._userName = userName;
		this._userEmail = userEmail;
	}

	public bool isValid()
	{
		var now = DateTime.Now;
		if (this._expires < now) { return false; }
		return true;
	}

}