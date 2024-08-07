var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddConnectionString("sql");

var dabService = builder.AddContainer("dab", "mcr.microsoft.com/azure-databases/data-api-builder")
    .WithHttpEndpoint(targetPort: 5000, name: "http")
    .WithBindMount(@"D:\src\distributed-dab-sk\aw-data-api-builder\dab-config.json", "/App/dab-config.json", true)
    .WithReference(sql)
    .WithOtlpExporter()
    .PublishAsContainer();
var dabServiceEndpoint = dabService.GetEndpoint("http");

builder.AddProject<Projects.DataAPIBuilder_AI_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(dabServiceEndpoint);

builder.Build().Run();
