var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddConnectionString("sqldb");

var dabService = builder.AddContainer("dab", "mcr.microsoft.com/azure-databases/data-api-builder")
    .WithHttpEndpoint(targetPort: 5000, name: "http")
    .WithBindMount(@"./dab-config.json", "/App/dab-bm0/dab-config.json", true)
    .WithArgs("--ConfigFileName", "./dab-bm0/dab-config.json")
    .WithReference(sql)
    .WithOtlpExporter()
    .PublishAsContainer();
var dabServiceEndpoint = dabService.GetEndpoint("http");

builder.AddProject<Projects.DataAPIBuilder_AI_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(dabServiceEndpoint);

builder.Build().Run();
