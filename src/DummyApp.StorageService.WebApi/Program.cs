using DummyApp.StorageService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddStorageServiceKeyVault(builder.Environment);
builder.Services.AddStorageServiceSettings(builder.Configuration);

builder.Services.AddStorageServicePersistence(builder.Configuration);
builder.Services.AddStorageServiceAuthentication(builder.Configuration);
builder.Services.AddStorageServiceAuthorization();
builder.Services.AddStorageServiceApi();

var app = builder.Build();

app.EnsureStorageServiceDatabase()
   .UseStorageServicePipeline();

app.Run();
