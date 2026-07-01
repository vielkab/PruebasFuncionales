var builder = DistributedApplication.CreateBuilder(args);

var database = builder
    .AddSqlServer("bdserver-test")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("bd-test");

builder.AddProject<Projects.Api>("api")
    .WithReference(database)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WaitFor(database);

builder.Build().Run();
