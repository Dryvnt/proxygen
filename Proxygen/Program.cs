using Microsoft.EntityFrameworkCore;
using SharedModel.Model;
using Update;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<ProxygenContext>(options => options.UseSqlite(connectionString));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

// Ensure DB is migrated
await using (var serviceScope = app.Services.CreateAsyncScope())
{
    await using (
        var cardContext = serviceScope.ServiceProvider.GetRequiredService<ProxygenContext>()
    )
    {
        await cardContext.Database.MigrateAsync();
    }
}

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();