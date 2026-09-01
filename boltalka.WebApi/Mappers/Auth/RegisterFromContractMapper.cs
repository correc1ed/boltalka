using boltalka.Application.Abstractions.Mappers;
using boltalka.Contracts.Models.Auth;

namespace boltalka.WebApi.Mappers.Auth;

public class RegisterFromContractMapper : MappingProfile
{
    public RegisterFromContractMapper()
    {
        CreateMap<RegisterContract, Application.Models.Auth.Register>(
            (source, mapper) => new Application.Models.Auth.Register()
            {
                Login = source.Login,
                DisplayName =  source.DisplayName,
                Password =  source.Password,
            });
    }
}