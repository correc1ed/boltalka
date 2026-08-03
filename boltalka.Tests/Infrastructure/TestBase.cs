using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Abstractions.Storage;
using boltalka.Application.Tests.Fakes;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Mappers.Call;
using boltalka.Infrastructure.Database.Mappers.Chat;
using boltalka.Infrastructure.Database.Mappers.ChatMember;
using boltalka.Infrastructure.Database.Mappers.Media;
using boltalka.Infrastructure.Database.Mappers.Message;
using boltalka.Infrastructure.Database.Mappers.MessageMedia;
using boltalka.Infrastructure.Database.Mappers.RefreshToken;
using boltalka.Infrastructure.Database.Mappers.User;
using boltalka.Infrastructure.Database.Profiles;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Application.Tests.Infrastructure;

public abstract class TestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ServiceProvider ServiceProvider;

    protected virtual void ConfigureServices(IServiceCollection services) { }
    
    protected TestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        
        services.AddDbContext<ServiceDbContext>(options =>
            options.UseSqlite(_connection));

        services.AddLogging();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "SuperSecretTestKey_AtLeast32Characters!",
                ["Jwt:Issuer"] = "boltalka-test",
                ["Jwt:Audience"] = "boltalka-test-client",
                ["Jwt:AccessTokenLifetimeMinutes"] = "5",
                ["Jwt:RefreshTokenLifetimeDays"] = "1"
            })
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);

        services.AddAutoMapper(cfg => { }, typeof(boltalka.Application.Abstractions.Mappers.MappingProfile).Assembly, typeof(EntityMappingProfile).Assembly);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ICallRepository, CallRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<ICallService, CallService>();
        services.AddScoped<IMediaService, MediaService>();

        services.AddScoped<INotificationService, FakeNotificationService>();

        services.AddScoped<IFileStorage, FakeFileStorage>();

        ConfigureServices(services);
        
        ServiceProvider = services.BuildServiceProvider();

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
    }
}