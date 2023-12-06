using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Domain.Enums;
using MedSecureSystem.Shared.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MedSecureSystem.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = services.GetRequiredService<MedSecureContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var applicationManager = services.GetRequiredService<OpenIddictApplicationManager<CustomApplication>>();

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(DbInitializer));

            // Check and apply any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            // Seed roles
            await SeedRolesAsync(roleManager, logger);

            // Seed default users
            await SeedDefaultUsersAsync(userManager, logger);
            await SeedSystemCredentials(applicationManager, logger);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var roleNames = EnumExtensions.GetEnumNames<UserTypes>();

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    logger.LogInformation($"Role {roleName} does not exist. Creating...");
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));

                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation($"Role {roleName} created successfully.");
                    }
                    else
                    {
                        logger.LogWarning($"Failed to create role {roleName}. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

        }

        private static async Task SeedDefaultUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var adminEmail = "systemadmin@medsecure.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                logger.LogInformation($"System Admin user does not exist. Creating...");
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    TenantKey = "SYSTEM",
                };

                string adminPassword = "AdminPassword123!";
                var createUser = await userManager.CreateAsync(adminUser, adminPassword);

                if (createUser.Succeeded)
                {
                    logger.LogInformation($"System Admin user created successfully.");
                    await userManager.AddToRoleAsync(adminUser, UserTypes.SystemAdmin.ToString());
                }
                else
                {
                    logger.LogWarning($"Failed to create System Admin user. Errors: {string.Join(", ", createUser.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation("System Admin user already exists.");
            }
        }

        private static async Task SeedSystemCredentials(OpenIddictApplicationManager<CustomApplication> applicationManager, ILogger logger)
        {
            // Generate client ID and secret.
            var clientId = "system-client";
            var clientSecret = "system-secret"; // This should be a strong, unique secret.

            var appl = await applicationManager.FindByClientIdAsync(clientId);

            /*["ept:token","ept:revocation","ept:authorization","gt:client_credentials","gt:password","gt:refresh_token","gt:authorization_code"]*/
            // Check if the application already exists.
            if (appl is null)
            {
                // Hash the client secret using SHA256.
                var hashedSecret = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(clientSecret)));

                var application = new CustomApplication
                {
                    ClientId = clientId,
                    DisplayName = "System Client Application",
                    IsActive = true,
                    BusinessKey = "SYSTEM",
                    Permissions = JsonSerializer.Serialize(new List<string>
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Revocation,
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                            OpenIddictConstants.Permissions.GrantTypes.Password,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    })
                };

                // Create the application in the OpenIddict database.
                await applicationManager.CreateAsync(application, hashedSecret);
                logger.LogInformation("Client Creation {result}:{secrete}", "done", hashedSecret);
            }
            else
            {
                logger.LogInformation("System Credential exist");
            }
        }
    }
}