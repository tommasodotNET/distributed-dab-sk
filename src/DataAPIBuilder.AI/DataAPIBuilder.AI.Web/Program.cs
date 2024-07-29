using DataAPIBuilder.AI.Web;
using DataAPIBuilder.AI.Web.Components;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddTransient( sp => {
    return Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion("gpt-4o", "https://oai-tstocchi-us.openai.azure.com/", "3599e954626f493bb34c4e68c270fcd9")
        .Build();
});

builder.Services.AddHttpClient("DABClient", client =>
{
    client.BaseAddress = new("https+http://dab");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
