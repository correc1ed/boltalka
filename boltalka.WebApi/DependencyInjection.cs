using boltalka.WebApi.Mappers.Auth;
using boltalka.WebApi.Mappers.Call;
using boltalka.WebApi.Mappers.Chat;
using boltalka.WebApi.Mappers.Media;
using boltalka.WebApi.Mappers.Message;
using boltalka.WebApi.Mappers.User;

namespace boltalka.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection RegisterMappersWebApiModels(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(
                typeof(AuthToContractMapper).Assembly,
                typeof(LoginFromContractMapper).Assembly,
                typeof(RegisterFromContractMapper).Assembly,
                
                typeof(CallFromContractMapper).Assembly,
                typeof(CallToContractMapper).Assembly,
                
                typeof(ChatFromContractMapper).Assembly,
                typeof(ChatToContractMapper).Assembly,
                
                typeof(MediaFromContractMapper).Assembly,
                typeof(MediaToContractMapper).Assembly,
                
                typeof(MessageFromContractMapper).Assembly,
                typeof(MessageToContractMapper).Assembly,
                
                typeof(UserFromContractMapper).Assembly,
                typeof(UserToContractMapper).Assembly
            );
        });
        
        return services;
    }
}