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

var openAIKey = builder.Configuration.GetSection("OpenAI").GetValue<string>("ApiKey")
                ?? throw new ArgumentNullException("OpenAI config not found");
var openAIEndpoint = builder.Configuration.GetSection("OpenAI").GetValue<string>("Endpoint")
                    ?? throw new ArgumentNullException("OpenAI config not found");

builder.Services.AddTransient( sp => {
    return Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion("gpt-4o", openAIEndpoint, openAIKey)
        .Build();
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
