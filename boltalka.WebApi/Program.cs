using System.Reflection;
using boltalka.Application;
using boltalka.Application.Abstractions.Services;
using boltalka.Infrastructure;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Storage;
using boltalka.WebApi.Hubs;
using boltalka.WebApi.UseCases.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ServiceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x =>
            x.MigrationsAssembly("boltalka.Infrastructure")
                .MigrationsHistoryTable("__MigrationsHistory", "boltalka")
                .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

builder.Services
    .AddApplication()
    .AddInfrastructureApplication()
    .RegisterRepositories()
    .RegisterMappersEntity();

// TODO : придумать какой нибудь другой способ интеграции сервиса уведомлений в SignalR.
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSignalR();

builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "boltalka v1");
        options.RoutePrefix = "api/v1/swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/", () => Results.Ok("Messenger API is running..."));
app.MapHub<ChatHub>("/hubs/chat");

app.Run();