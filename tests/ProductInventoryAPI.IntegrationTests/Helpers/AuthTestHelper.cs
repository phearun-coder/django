using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProductInventoryAPI.IntegrationTests.Infrastructure;

namespace ProductInventoryAPI.IntegrationTests.Helpers;

public static class AuthTestHelper
{
    private const string TestSecretKey = "ThisIsATestSecretKeyForJWTTokenGenerationInIntegrationTests12345";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";

    public static string GenerateJwtToken(string userId = "1", 
                                         string username = "testuser", 
                                         string role = "User",
                                         int expirationMinutes = 60)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(TestSecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new("username", username),
            new("userId", userId)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateAdminToken()
    {
        return GenerateJwtToken(
            userId: "2",
            username: "admin",
            role: "Admin"
        );
    }

    public static HttpClient GetAuthenticatedClient(IntegrationTestWebAppFactory factory, string? token = null)
    {
        var client = factory.CreateClient();
        
        token ??= GenerateJwtToken();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        return client;
    }

    public static HttpClient GetAuthenticatedAdminClient(IntegrationTestWebAppFactory factory)
    {
        return GetAuthenticatedClient(factory, GenerateAdminToken());
    }
}