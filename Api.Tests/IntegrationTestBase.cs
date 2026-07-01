using Microsoft.Extensions.Logging;

namespace Api.Tests;

public abstract class IntegrationTestBase
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(90);
    private DistributedApplication? app;

    protected HttpClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task StartApplicationAsync()
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Api_Tests_AppHost>(cancellationToken);

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        Client = app.CreateHttpClient("api");
    }

    [OneTimeTearDown]
    public async Task StopApplicationAsync()
    {
        Client.Dispose();

        if (app is not null)
        {
            await app.DisposeAsync();
        }
    }
}
