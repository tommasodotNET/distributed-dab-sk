var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.DataAPIBuilder_AI_ApiService>("apiservice");

builder.AddProject<Projects.DataAPIBuilder_AI_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
