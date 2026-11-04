using NovelsSearchApp.Components;
using NovelsSearchApp.Components.Pages.UserInput;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Begin HTTP client code.
builder.Services.AddHttpClient("NarouAPI", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api.syosetu.com/");//I think this URI cover R18 API.

    //Set a user agent to avoid being blocked by the server.
    httpClient.DefaultRequestHeaders.Add("User-Agent", "NovelSearchApp/2.0");
});
//End of HTTP client code.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
