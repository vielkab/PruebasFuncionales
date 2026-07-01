var builder = DistributedApplication.CreateBuilder(args);

builder.AddSqlServer("bdserver-test")
    .AddDatabase("bd");

builder.Build().Run();
