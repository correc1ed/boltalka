using System.Reflection;
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

app.Run();