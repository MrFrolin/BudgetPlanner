using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetPlanner.Server.Services;

public interface ITokenService
{
    Task<string> RefreshIdTokenAsync(string refreshToken);
    bool TokenIsExpired(string idToken);
}

public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClient;

    public TokenService(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<string> RefreshIdTokenAsync(string refreshToken)
    {
        var client = _httpClient.CreateClient("RefreshTokenAPI");

        var requestData = new
        {
            grant_type = "refresh_token",
            refresh_token = refreshToken
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestData),
        Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("",
            jsonContent
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to refresh token.");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<RefreshTokenResponse>(responseJson);

        return responseData.IdToken;
    }

    public bool TokenIsExpired(string idToken)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(idToken) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

        return jsonToken.ValidTo < DateTime.UtcNow;
    }

    public class RefreshTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public string ExpiresIn { get; set; }
    }

}   


