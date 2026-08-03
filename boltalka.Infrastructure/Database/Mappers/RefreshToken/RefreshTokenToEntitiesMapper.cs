using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Mappers.RefreshToken;

public class RefreshTokenToEntitiesMapper : MappingProfile
{
    public RefreshTokenToEntitiesMapper()
    {
        CreateMap<Application.Models.RefreshToken.RefreshToken, Entities.RefreshTokenEntity>(
            (source, mapper) => new Entities.RefreshTokenEntity
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt =  source.UpdatedAt,
                Token = source.Token,
                ExpiresAt = source.ExpiresAt,
                UserId = source.UserId,
                //User = mapper.Map<UserEntity>(source.User),
            });
    }
}