var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.XE_NetConf2024_ApiService>("apiservice");

builder.AddProject<Projects.XE_NetConf2024_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
