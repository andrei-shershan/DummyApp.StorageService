using Azure.Identity;
using DummyApp.StorageService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Key Vault: add in stg/prod only; local dev uses appsettings.Development.json
if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
    }
}

// Add services to the container.

builder.Services.AddControllers();

var databaseSection = builder.Configuration.GetSection("Database");
var useInMemoryDb = databaseSection.GetValue<bool?>("UseInMemory") ?? true;
var connectionString = databaseSection.GetValue<string>("ConnectionString");

builder.Services.AddDbContext<StorageDbContext>(options =>
{
    if (useInMemoryDb)
    {
        options.UseInMemoryDatabase("StorageDb");
    }
    else
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is required when Database:UseInMemory is false.");
        }

        options.UseMySQL(connectionString, sqlOptions => sqlOptions.MigrationsAssembly("DummyApp.StorageService.Data"));
    }
});

var authConfig = builder.Configuration.GetSection("Authentication");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = authConfig["Authority"];
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = authConfig["Audience"],
            ValidateIssuer = true,
            ValidIssuer = authConfig["Authority"]
        };
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
if (builder.Configuration.GetValue<bool>("ReverseProxy:TrustAllProxies"))
{
    // Dev only: trust all proxies inside the Docker network (Traefik).
    // Do NOT enable in production.
    forwardedOptions.KnownNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
}
app.UseForwardedHeaders(forwardedOptions);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
