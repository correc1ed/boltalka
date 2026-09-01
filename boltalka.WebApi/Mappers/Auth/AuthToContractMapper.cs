using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.Auth;
using boltalka.Contracts.Models.Auth;

namespace boltalka.WebApi.Mappers.Auth;

public class AuthToContractMapper : MappingProfile
{
    public AuthToContractMapper()
    {
        CreateMap<AuthTokens, AuthTokensContract>(
            (source, mapper) => new AuthTokensContract()
            {
                AccessToken = source.AccessToken,
                RefreshToken = source.RefreshToken,
            });
    }
}