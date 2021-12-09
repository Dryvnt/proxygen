using Microsoft.EntityFrameworkCore;
using SharedModel.Model;

// This program is entirely just so that the migrations builder knows which DB engine to use.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CardContext>(options => options.UseNpgsql("Dummy Connection String :)"));

var app = builder.Build();
app.Run();