using System.Net.Http.Headers;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Common;

public abstract class BaseIntegrationTest : IDisposable
{
    private readonly IServiceScope _scope;
    protected ApplicationDbContext Context { get; }
    protected HttpClient Client { get; }

    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
        _scope = factory.Services.CreateScope();
        Context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        Client.Dispose();
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
