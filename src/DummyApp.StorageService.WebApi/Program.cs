using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var authConfig = builder.Configuration.GetSection("Authentication");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var authority = authConfig["Authority"];
        var metadataAddress = authConfig["MetadataAddress"];

        // Authority is the token issuer visible to clients.
        // MetadataAddress may point to the internal Docker service address for container-to-container discovery.
        options.Authority = authority;
        if (!string.IsNullOrEmpty(metadataAddress))
        {
            options.MetadataAddress = metadataAddress;
        }

        options.RequireHttpsMetadata = authority?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ?? false;
        options.TokenValidationParameters.ValidateAudience = false; // TODO: In production, validate audience and scopes as needed
        options.TokenValidationParameters.ValidIssuer = authority;
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("RequireStorageRead", policy =>
        policy.RequireClaim("scope", "storage.read").RequireAuthenticatedUser());

    options.AddPolicy("RequireStorageWrite", policy =>
        policy.RequireClaim("scope", "storage.write").RequireAuthenticatedUser());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
