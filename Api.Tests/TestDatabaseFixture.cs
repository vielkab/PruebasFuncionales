using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

[SetUpFixture]
public sealed class TestDatabaseFixture
{
    private static Aspire.Hosting.DistributedApplication? appHost;
    private static WebApplicationFactory<Api.Program>? factory;

    public static HttpClient CreateClient() =>
        factory?.CreateClient()
        ?? throw new InvalidOperationException("La fabrica web no esta inicializada.");

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        var appHostBuilder =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.Api_Tests_AppHost>();

        appHost = await appHostBuilder.BuildAsync();
        await appHost.StartAsync();

        var connectionString = await appHost.GetConnectionStringAsync("bd");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se pudo obtener la cadena de conexion de SQL Server.");
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__bd", connectionString);

        factory = new WebApplicationFactory<Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });

        _ = factory.Services;
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        if (appHost is not null)
        {
            await appHost.DisposeAsync();
        }
    }
}
