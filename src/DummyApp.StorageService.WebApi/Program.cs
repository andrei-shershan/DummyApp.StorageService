using Azure.Identity;
using DummyApp.StorageService.Data;
using DummyApp.StorageService.WebApi.Services;
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
        var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
        var credential = string.IsNullOrEmpty(clientId)
            ? new ManagedIdentityCredential()
            : new ManagedIdentityCredential(clientId);

        builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
    }
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IArtworkService, ArtworkService>();

var databaseSection = builder.Configuration.GetSection("Infrastructure:Databases:Storage");
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = builder.Configuration["IdentityServer:Audience"],
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["IdentityServer:Authority"]
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

    options.AddPolicy("CreateArtwork", policy =>
        policy.RequireAssertion(context =>
            context.User.Identity?.IsAuthenticated == true &&
            (context.User.HasClaim("role", "Creator") || context.User.HasClaim("scope", "storage.write"))));
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
