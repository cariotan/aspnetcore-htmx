using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

public abstract class ExternalAuthService
{
	protected string providerName;

	protected string loginUrl;

	protected string callbackUrl;

	public ExternalAuthService(string providerName, string loginUrl, string callbackUrl)
	{
		this.providerName = providerName;
		this.loginUrl = loginUrl;
		this.callbackUrl = callbackUrl;
	}

	public abstract string GetLoginUrl(string redirectUrl, string state);
	public abstract Task<Result<ExternalAuthUserInfo>> GetIdToken(string code, string state, HttpClient httpClient);
}

public class GoogleAuthService : ExternalAuthService
{
	private string? state;
	private string clientId;
	private string clientSecret;

	public GoogleAuthService(string clientId, string clientSecret)
		: base("google", "https://accounts.google.com/o/oauth2/v2/auth", "https://oauth2.googleapis.com/token")
	{
		this.clientId = clientId;
		this.clientSecret = clientSecret;
	}

	public override string GetLoginUrl(string redirectUrl, string state)
	{
		this.state = state;
		return $"{loginUrl}?response_type=code&client_id={GetEnvironmentVariable("google-client-id")}&redirect_uri={redirectUrl}&scope=openid email profile&state={state}";
	}

	public override async Task<Result<ExternalAuthUserInfo>> GetIdToken(string code, string state, HttpClient httpClient)
	{
		if (state == this.state)
		{
			var response = await httpClient.PostAsync(callbackUrl, new FormUrlEncodedContent(new Dictionary<string, string>()
			{
				["grant_type"] = "authorization_code",
				["code"] = code,
				["redirect_uri"] = callbackUrl,
				["client_id"] = clientId,
				["client_secret"] = clientSecret,
			}));

			var googleResponse = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<GoogleResponse>();

			var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(googleResponse!.IdToken);

			var sub = jwtToken.Claims.First(x => x.Type == "sub").Value;
			var email = jwtToken.Claims.First(x => x.Type == "email").Value;

			return new ExternalAuthUserInfo(sub, email);	
		}
		else
		{
			return new Error("Invalid state");
		}
	}
	
	private class GoogleResponse
	{
		[JsonPropertyName("id_token")]
		public required string IdToken { get; set; }

		[JsonPropertyName("access_token")]
		public required string AccessToken { get; set; }
	}
}

public record ExternalAuthUserInfo(string Id, string Email);