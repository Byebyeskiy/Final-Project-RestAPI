using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Common;

public sealed class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public Task InitializeAsync() => _database.StartAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _database.GetConnectionString());
        builder.ConfigureTestServices(services =>
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { }));
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        try
        {
            await DisposeAsync();
        }
        finally
        {
            await _database.DisposeAsync();
        }
    }
}
