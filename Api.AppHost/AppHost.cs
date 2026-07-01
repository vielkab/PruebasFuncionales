var builder = DistributedApplication.CreateBuilder(args);

var database = builder
    .AddSqlServer("bdserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("bd");

builder.AddProject<Projects.Api>("api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
