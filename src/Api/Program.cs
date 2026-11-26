using Api.Modules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.SetupServices();
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, AdminAuthenticationHandler>(
        AdminAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Resale Marketplace API", Version = "v1" });
    options.AddSecurityDefinition(AdminAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = AdminAuthenticationHandler.HeaderName,
        Description = "Configured moderation administrator key."
    });
    options.OperationFilter<AdminSwaggerOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.InitialiseDatabaseAsync();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
