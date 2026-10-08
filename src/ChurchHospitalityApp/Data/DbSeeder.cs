using ChurchHospitalityApp.Models;
using Microsoft.AspNetCore.Identity;

namespace ChurchHospitalityApp.Data
{
    /// <summary>
    /// Ensures both roles exist and that a default Admin exists so the first person
    /// can log in and start adding members. Change the default password after first login.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { AppRoles.Admin, AppRoles.User })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminSection = config.GetSection("App:DefaultAdmin");
            var adminEmail = adminSection["Email"];
            var adminPassword = adminSection["Password"];
            var adminFullName = adminSection["FullName"] ?? "System Administrator";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            if (await userManager.FindByEmailAsync(adminEmail) != null)
            {
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
    }
}
