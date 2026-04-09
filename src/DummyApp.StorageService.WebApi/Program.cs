using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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
