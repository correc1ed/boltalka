using boltalka.Application.Abstractions.Services;
using boltalka.Application.Mappers.Chat;
using boltalka.Application.Mappers.Message;
using boltalka.Application.Mappers.User;
using boltalka.Application.UseCases.Services;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Application DI.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICallService, CallService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
    
    public static IServiceCollection RegisterMappersApplicationModels(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(
                typeof(ChatToChatInfoMapper).Assembly,
                typeof(ChatToChatListItemMapper).Assembly,
                
                typeof(MessageToMessageInfoMapper).Assembly,
                
                typeof(RegisterToUserMapper).Assembly,
                typeof(UserToUserProfileMapper).Assembly
            );
        });
        
        return services;
    }
}