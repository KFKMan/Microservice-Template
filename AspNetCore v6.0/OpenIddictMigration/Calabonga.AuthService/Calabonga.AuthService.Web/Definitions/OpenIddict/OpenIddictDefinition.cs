﻿using Calabonga.AuthService.Infrastructure;
using Calabonga.AuthService.Web.Definitions.Base;
using Calabonga.AuthService.Web.HostedServices;
using OpenIddict.Abstractions;

namespace Calabonga.AuthService.Web.Definitions.OpenIddict
{
    public class OpenIddictDefinition : AppDefinition
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                    options.UseEntityFrameworkCore()
                        .UseDbContext<ApplicationDbContext>()
                        .ReplaceDefaultEntities<Guid>();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Note: the sample uses the code and refresh token flows but you can enable
                    // the other flows if you need to support implicit, password or client credentials.
                    // Supported flows are:
                    //  => Authorization code flow
                    //  => Client credentials flow
                    //  => Device code flow
                    //  => Implicit flow
                    //  => Password flow
                    //  => Refresh token flow
                    options
                        .AllowAuthorizationCodeFlow()//.RequireProofKeyForCodeExchange()
                        .AllowPasswordFlow()
                        .AllowClientCredentialsFlow()
                        .AllowRefreshTokenFlow();

                    // Using reference tokens means the actual access and refresh tokens
                    // are stored in the database and different tokens, referencing the actual
                    // tokens (in the db), are used in request headers. The actual tokens are not
                    // made public.
                    // => options.UseReferenceAccessTokens();
                    // => options.UseReferenceRefreshTokens();

                    // Set the lifetime of your tokens
                    // => options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                    // => options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    // Enable the token endpoint.
                    options
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo");

                    // Encryption and signing of tokens
                    options
                        .AddEphemeralEncryptionKey() // only for Developing mode
                        .AddEphemeralSigningKey() // only for Developing mode
                        .DisableAccessTokenEncryption(); // only for Developing mode

                    // Mark the "email", "profile" and "roles" scopes as supported scopes.
                    options.RegisterScopes(
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles,
                        "api",
                        "custom");

                    // Register the signing and encryption credentials.
                    options
                        .AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options
                        .UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });

            // Register the worker responsible for seeding the database.
            // Note: in a real world application, this step should be part of a setup script.
            services.AddHostedService<OpenIddictWorker>();
        }
    }
}