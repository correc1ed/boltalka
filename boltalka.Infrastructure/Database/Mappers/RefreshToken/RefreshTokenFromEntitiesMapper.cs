using boltalka.Application.Abstractions.Mappers;

namespace boltalka.Infrastructure.Database.Mappers.RefreshToken;

public class RefreshTokenFromEntitiesMapper : MappingProfile
{
    public RefreshTokenFromEntitiesMapper()
    {
        CreateMap<Entities.RefreshTokenEntity, Application.Models.RefreshToken.RefreshToken>(
            (source, mapper) => new Application.Models.RefreshToken.RefreshToken
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt =  source.UpdatedAt,
                Token = source.Token,
                ExpiresAt = source.ExpiresAt,
                UserId = source.UserId,
                //User = mapper.Map<Application.Models.User.User>(source.User),
            });
    }
}