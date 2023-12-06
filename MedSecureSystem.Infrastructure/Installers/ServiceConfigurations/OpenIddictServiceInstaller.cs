using MedSecureSystem.Domain.Entities;
using MedSecureSystem.Infrastructure.Data;
using MedSecureSystem.Shared.Attributes;
using MedSecureSystem.Shared.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Validation.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MedSecureSystem.Infrastructure.Installers.ServiceConfigurations;

[Installer(position: 4)]
public class OpenIddictServiceInstaller : IServiceInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
            options.ClaimsIdentity.EmailClaimType = Claims.Email;
        });

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                // Configure OpenIddict to use the EF Core stores and models.
                options.UseEntityFrameworkCore()
                    .UseDbContext<MedSecureContext>()
                    .ReplaceDefaultEntities<CustomApplication, CustomAuthorization, CustomScope, CustomToken, string>();
            })
            .AddServer(options =>
            {
                // Enable the token endpoint.
                options.SetTokenEndpointUris("/connect/token");

                // Enable the password and the refresh token flows.
                options.AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow()
                    .DisableAccessTokenEncryption();

                // Accept anonymous clients (i.e., clients that don't send a client_id).
                options.AcceptAnonymousClients();

                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.AddEncryptionKey(
                       new SymmetricSecurityKey(
                           Convert.FromBase64String("DmtGvqInBpDJQXpanMkq2IofSKrtmYdlt7tUNSXTC4I="))
                       );

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(10));
                options.SetRefreshTokenLifetime(TimeSpan.FromHours(24));


                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough();
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                options.AddEncryptionKey(new SymmetricSecurityKey(
                    Convert.FromBase64String("DmtGvqInBpDJQXpanMkq2IofSKrtmYdlt7tUNSXTC4I=")));
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddCors(options =>
        {
            options.AddPolicy(
                name: "BaseCorsPolicy",
                policy =>
                {
                    policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                }
            );
        });
    }
}
