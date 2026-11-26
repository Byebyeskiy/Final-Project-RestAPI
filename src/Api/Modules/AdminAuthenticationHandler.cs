using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Modules;

public sealed class AdminAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "AdminKey";
    public const string HeaderName = "X-Admin-Key";
    public const string RoleName = "Admin";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var supplied))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var expected = configuration["Moderation:AdminKey"];
        if (expected is null || expected.Length < 32 || supplied.Count != 1 ||
            !CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
                SHA256.HashData(Encoding.UTF8.GetBytes(supplied.ToString()))))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid administrator credentials."));
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "moderation-admin"), new Claim(ClaimTypes.Role, RoleName)],
            SchemeName);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
