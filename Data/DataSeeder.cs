using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Models;

namespace UrlShortener.Data;

public static class DataSeeder
{
    private const string AdminRole = "Admin";
    private const string UserRole = "User";
    
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin";
    private const string AdminEmail = "admin@admin.com";
    
    private const string RegularUsername = "user";
    private const string RegularPassword = "user";
    private const string RegularEmail = "user@user.com";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        
        try
        {
            // Ensure database is created
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            logger.LogInformation("Ensuring database exists...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database ready.");

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create roles
            await EnsureRoleExistsAsync(roleManager, AdminRole, logger);
            await EnsureRoleExistsAsync(roleManager, UserRole, logger);

            // Create default users
            await EnsureUserExistsAsync(
                userManager, 
                AdminUsername, 
                AdminEmail, 
                AdminPassword, 
                AdminRole, 
                logger);
                
            await EnsureUserExistsAsync(
                userManager, 
                RegularUsername, 
                RegularEmail, 
                RegularPassword, 
                UserRole, 
                logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager, 
        string roleName, 
        ILogger logger)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            logger.LogInformation("{Role} role created.", roleName);
        }
    }

    private static async Task EnsureUserExistsAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string email,
        string password,
        string role,
        ILogger logger)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = username,
                Email = email
            };
            
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation("Default {Role} user '{Username}' created.", role, username);
            }
            else
            {
                logger.LogWarning(
                    "Failed to create {Role} user '{Username}': {Errors}", 
                    role, 
                    username, 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}

