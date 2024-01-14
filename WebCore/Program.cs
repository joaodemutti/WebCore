using WebCore.Game;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();
builder.Services.AddMvc();
builder.Services.AddControllers().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

//if(!builder.Environment.IsDevelopment())
//    builder.WebHost.UseUrls("https://192.168.0.104/");

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
