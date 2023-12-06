using MedSecureSystem.API.Middleware;
using MedSecureSystem.Application;
using MedSecureSystem.Application.Commons;
using MedSecureSystem.Domain.Enums;
using MedSecureSystem.Infrastructure;
using MedSecureSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = context =>
                        {
                            var errors = context.ModelState
                                .Where(e => e.Value.Errors.Count > 0)
                                .SelectMany(x => x.Value.Errors)
                                .Select(x => x.ErrorMessage).ToList();

                            var result = new ApiResult<object>(null, false, "Model validation error")
                            {
                                Errors = errors
                            };

                            return new BadRequestObjectResult(result);
                        };
                    });

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();



builder.Services.RegisterApplicationService();
builder.Services.RegisterInfrastructureService(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSystemAdminRole", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == ClaimTypes.Role && c.Value == UserTypes.SystemAdmin.ToString())));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseErrorHandler();

app.UseHttpsRedirection();

app.UseCors("BaseCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.RegisterInfrastructureApplicationBuilder();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var services = scope.ServiceProvider;
    // Call the Initialize method of the DbInitializer class
    await DbInitializer.Initialize(services);
}

app.Run();

