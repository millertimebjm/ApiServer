using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SilvermineNordic.Admin;
using SilvermineNordic.Admin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = "http://localhost:9080";
var hostEnvironment = builder.Services.BuildServiceProvider().GetService<IWebAssemblyHostEnvironment>();
if (hostEnvironment.IsProduction())
{
    //apiUrl = "https://silverminenordicapi.azurewebsites.net";
    apiUrl = "https://miller.silverminenordic.com:9443";
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ICookie, Cookie>();
builder.Services.AddSingleton<IGlobalService>(_ => new GlobalService(apiUrl));

await builder.Build().RunAsync();
