namespace Api.Tests;

public abstract class IntegrationTestBase
{
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void CreateClient()
    {
        Client = TestDatabaseFixture.CreateClient();
    }

    [TearDown]
    public void DisposeClient()
    {
        Client.Dispose();
    }
}
