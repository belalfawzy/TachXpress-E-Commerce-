using Microsoft.AspNetCore.Identity;
using TechXpress_DepiGraduation.Data.StaticMembers;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Seed
{
    public class AppDbInitializer
    {
        public static async Task SeedUserAndRoleAsync(IApplicationBuilder applicationBuilder)
        {
            using (var service = applicationBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = service.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create roles if they don't exist
                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                }
                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                }

                // ✅ Use AppUser here
                var userManager = service.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                // Seed Admin
                if (await userManager.FindByEmailAsync("admin@admin.com") == null)
                {
                    var adminUser = new AppUser()
                    {
                        Email = "admin@admin.com",
                        EmailConfirmed = true,
                        FullName = "Admin User",
                        UserName = "Admin",
                        PhoneNumber = "1234567890",
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                        await userManager.AddToRoleAsync(adminUser, UserRoles.User);
                    }
                }

                // Seed Normal User
                if (await userManager.FindByEmailAsync("user@user.com") == null)
                {
                    var appUser = new AppUser()
                    {
                        Email = "User@user.com",
                        EmailConfirmed = true,
                        FullName = "Normal User",
                        UserName = "User",
                        PhoneNumber = "0123456789"
                    };
                    var result = await userManager.CreateAsync(appUser, "Us@r123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(appUser, UserRoles.User);
                    }
                }
            }
        }
    }
}