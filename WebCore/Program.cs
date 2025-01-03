using WebCore.Game;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();
builder.Services.AddMvc();
builder.Services.AddControllers().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

builder.WebHost.UseUrls("http://+:8080");

var app = builder.Build();

app.MapHub<GameHub>("/Game");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
