using Microsoft.EntityFrameworkCore;
using NodaTime;
using SharedModel.Model;
using Update;
using Update.Options;
using Update.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<ProxygenContext>(options =>
    options.UseSqlite(connectionString, x => x.UseNodaTime())
);

builder.Services.Configure<ProxygenUpdaterOptions>(
    builder.Configuration.GetSection(ProxygenUpdaterOptions.ProxygenUpdater)
);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddScoped<IScryfallFetcher, ScryfallFetcher>();
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddHostedService<BackgroundProxygenUpdater>();

var app = builder.Build();

// Ensure DB is migrated
await using (var serviceScope = app.Services.CreateAsyncScope())
{
    await using var db = serviceScope.ServiceProvider.GetRequiredService<ProxygenContext>();
    await db.Database.MigrateAsync();
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
