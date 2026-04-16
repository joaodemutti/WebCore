using WebCore.Game;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();
builder.Services.AddMvc();
builder.Services.AddControllers().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

var portValue = Environment.GetEnvironmentVariable("PORT");
var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 80;
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();

app.MapHub<GameHub>("/Game");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
