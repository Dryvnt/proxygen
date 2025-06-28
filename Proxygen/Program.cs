using Microsoft.EntityFrameworkCore;
using NodaTime;
using Proxygen.Services;
using SharedModel.Model;
using Update;
using Update.Options;
using Update.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<ProxygenContext>(options =>
{
    options.UseSqlite(connectionString, x => x.UseNodaTime());
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.Configure<ProxygenUpdaterOptions>(
    builder.Configuration.GetSection(ProxygenUpdaterOptions.ProxygenUpdater)
);

builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddScoped<IScryfallFetcher, ScryfallFetcher>();
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddHostedService<BackgroundProxygenUpdater>();
builder.Services.AddHttpClient<IScryfallFetcher, ScryfallFetcher>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Proxygen/1.0");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json;q=0.9,*/*;q=0.8");
});

builder.Services.AddScoped<CardSearcher>();

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

app.MapDefaultControllerRoute();

app.Run();
