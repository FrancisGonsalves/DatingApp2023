using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace API.Data
{
    public class Seed
    {
        public static async Task RemoveConnections(DataContext context)
        {
            context.Connections.RemoveRange(context.Connections);
            await context.SaveChangesAsync();
        }
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if(await userManager.Users.AnyAsync()) return;

            string json = await File.ReadAllTextAsync("Data/UserSeedData.json");

            JsonSerializerOptions option = new JsonSerializerOptions();
            option.PropertyNameCaseInsensitive = true;

            List<AppUser> users = JsonSerializer.Deserialize<List<AppUser>>(json);

            List<AppRole> roles = new List<AppRole> {
                new AppRole{ Name = "Member" },
                new AppRole{ Name = "Admin" },
                new AppRole { Name = "Moderator" }
            };

            foreach(AppRole role in roles)
                await roleManager.CreateAsync(role);

            foreach(AppUser user in users)
            {
                user.UserName = user.UserName.ToLower();
                user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }

            AppUser admin = new AppUser { UserName = "admin" };
            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
        }
    }
}