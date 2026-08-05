using boltalka.Application.Abstractions.Repositories;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Mappers;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Infrastructure;

public static class InfrastructureRegister
{
    public static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        // Если ServiceDbContext наследует DbContext, можно зарегистрировать его напрямую.
        // Но фабрика тоже допустима, оставляем как есть.
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<ServiceDbContext>());
        
        services.AddScoped<ICallRepository, CallRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        
        return services;
    }
    public static IServiceCollection RegisterMappersEntity(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(
                typeof(CallFromEntitiesMapper).Assembly,
                typeof(CallToEntitiesMapper).Assembly,
                
                typeof(ChatFromEntitiesMapper).Assembly,
                typeof(ChatToEntitiesMapper).Assembly,
                
                typeof(ChatMemberFromEntitiesMapper).Assembly,
                typeof(ChatMemberToEntitiesMapper).Assembly,
                
                typeof(MediaFromEntitiesMapper).Assembly,
                typeof(MediaToEntitiesMapper).Assembly,
                
                typeof(MessageFromEntitiesMapper).Assembly,
                typeof(MessageToEntitiesMapper).Assembly,
                
                typeof(MessageMediaFromEntitiesMapper).Assembly,
                typeof(MessageMediaToEntitiesMapper).Assembly,
                
                typeof(UserFromEntitiesMapper).Assembly,
                typeof(UserToEntitiesMapper).Assembly,
                
                typeof(RefreshTokenToEntitiesMapper).Assembly,
                typeof(RefreshTokenFromEntitiesMapper).Assembly,
                
                typeof(EntityMappingProfile).Assembly
                );
        });
        
        return services;
    }
}