using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API.Extensions;
using System.Security.Cryptography;
using API.Middleware;
using Microsoft.AspNetCore.Identity;
using API.Entities;
using API.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<MiddlewareException>();

app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

var scoped = app.Services.CreateScope();
try
{
    var context = scoped.ServiceProvider.GetRequiredService<DataContext>();
    var userManager = scoped.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scoped.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await Seed.RemoveConnections(context);
    await Seed.SeedUsers(userManager, roleManager);
}
catch(Exception ex) {
    ILogger<Program> logger = scoped.ServiceProvider.GetService<ILogger<Program>>();
    logger.LogError(ex, "An Error Occured during Migration.");
}

app.Run();
