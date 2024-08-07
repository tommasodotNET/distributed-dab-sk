# Data API Builder and Semantic Kernel with Aspire

This Repository contains the source code for the Data API Builder and Semantic Kernel, a solution that allows users to create APIs for their data sources and to query them using a semantic kernel. The tool is built using .NET Aspire.

## Requirements

To start the project, you need to have the following tools installed:
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio)
- [Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd?tabs=winget-windows%2Cbrew-mac%2Cscript-linux&pivots=os-windows)
- Container Engine (e.g. Docker)

To configure and test Data API Builder, you need to [install the `dab cli`](https://learn.microsoft.com/en-us/azure/data-api-builder/how-to-install-cli).

## How To

To get started with Data API Builder, refer to the [official documentation](https://learn.microsoft.com/en-us/azure/data-api-builder/).

To run the project locally, you need to configure few variables in the appsettings:
- the database connection string in the `appsettings.Development.json` file of `DataAPIBuilder.AI.AppHost` project:
```json
"ConnectionStrings": {
    "sqldb": "<CONNECTION_STRING>"
}
```
- The Endpoint and API Key for Azure OpenAI in the `appsettings.Development.json` file of `DataAPIBuilder.AI.Web` project:
```json
"OpenAI": {
    "Endpoint": "https://<AOAI_NAME>.openai.azure.com/",
    "ApiKey": "<API_KEY>"
}
```

## Notes on Deployment

The solution can be deployed using the [Azure Developer CLI](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/overview) - which I will from know on call azd.
To learn how azd and .NET Aspire work together, refer to the [official documentation](https://learn.microsoft.com/en-us/dotnet/aspire/deployment/azure/aca-deployment-github-actions?tabs=windows&pivots=github-actions).

> **_NOTE:_**  The database won't be deployed by azd, as it is only referenced as a connection string in the AppHost project.

After running the `azd init` command, we cannot proceed to run `azd up` because there are a few to change in the Azure Container Apps templates. Therefore, we need to run the following command:
```bash
azd infra synth
```

> **_NOTE:_**  To run `azd infra synth` we need to enable the feature since it is still in alpha by running `azd config set alpha.infraSynth on`.

When running azd infra synth, the folders containing the biceps and the yaml templates for the Azure Container Apps will be created in two infra folders.

### Avoiding the issue with sqldb connection string parameter

Since we not declaring the Database with the AppHost, the template for the dab's Azure Container App won't show the correct value for sqldb connection string. To fix this, we need to add the following variable in [.azure/distributed-dab-sk/.env](./src/DataAPIBuilder.AI/.azure/distributed-dab-sk/.env):
```bash
AZURE_SQLDB = "<CONNECTION_STRING>"
```

After adding the variable, we can update the value for the sqldb connection string in the [dab template file](./src/DataAPIBuilder.AI/DataAPIBuilder.AI.AppHost/infra/dab.tmpl.yaml) with the following value:
```yaml
    secrets:
      - name: connectionstrings--sqldb
        value: '{{ .Env.AZURE_SQLDB }}'
```

This will allow the Azure Container App to have the correct value for the sqldb connection string.

### Configuring Data API Builder to find the configuration file

When the Data API Builder container starts, it will look for a configuration file, usually name dab-config.json.

In the AppHost project, Data API Builder is configured with a BindMount as follows:
```csharp
var dabService = builder.AddContainer("dab", "mcr.microsoft.com/azure-databases/data-api-builder")
    .WithHttpEndpoint(targetPort: 5000, name: "http")
    .WithBindMount(@"./dab-config.json", "/App/dab-bm0/dab-config.json", true)
    .WithArgs("--ConfigFileName", "./dab-bm0/dab-config.json")
    .WithReference(sql)
    .WithOtlpExporter()
    .PublishAsContainer();
```

From release [1.9.4](https://github.com/Azure/azure-dev/releases/tag/azure-dev-cli_1.9.4), azd cli supports the [binding mount for Aspire](https://github.com/Azure/azure-dev/pull/3790). Unfortunatlly, the binding mount is not working as expected, and the configuration file is not being found by the Data API Builder container.

We need to change the configuration for the volume bind in the [dab template file](./src/DataAPIBuilder.AI/DataAPIBuilder.AI.AppHost/infra/dab.tmpl.yaml) AND get rid of the azd.operations.yaml file that azd cli creates. This file should be in charge of uploading the file to the shared volume on Azure, but it is not able to find the file nor it is able to upload it.

The following changes need to be made in the dab template file:
```yaml
volumeMounts:
  - volumeName: <VOLUME_MOUNT_NAME>
    mountPath: /App/<FILE_SHARE_NAME>
args:
  - --ConfigFileName=./<FILE_SHARE_NAME>/dab-config.json
```

After running `azd up`, you will of course need to upload the configuration file to the shared volume.

If you let azd cli provision the infrastructure, `<FILE_SHARE_NAME>` and `<VOLUME_MOUNT_NAME>` will have the same value.