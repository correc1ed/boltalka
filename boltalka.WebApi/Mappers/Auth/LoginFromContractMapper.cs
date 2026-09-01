using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.Auth;
using boltalka.Contracts.Models.Auth;

namespace boltalka.WebApi.Mappers.Auth;

public class LoginFromContractMapper : MappingProfile
{
    public LoginFromContractMapper()
    {
        CreateMap<CredentialsContract, Credentials>(
            (source, mapper) => new Application.Models.Auth.Credentials()
            {
                Login =  source.Login,
                Password =  source.Password,
            });
    }
}