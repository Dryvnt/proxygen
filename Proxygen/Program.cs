using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

builder.Services.AddDbContext<CardContext>();

if (builder.Configuration.GetValue<bool>("Updater:Enabled"))
{
    builder.Services.AddHttpClient();
    builder.Services.AddHostedService<Worker>();
}

var app = builder.Build();

// Ensure DB is migrated
await using (var serviceScope = app.Services.CreateAsyncScope())
{
    await using (var cardContext = serviceScope.ServiceProvider.GetRequiredService<CardContext>())
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