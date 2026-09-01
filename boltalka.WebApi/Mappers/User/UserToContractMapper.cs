using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.User;
using boltalka.Contracts.Models.User;

namespace boltalka.WebApi.Mappers.User;

public class UserToContractMapper : MappingProfile
{
    public UserToContractMapper()
    {
        CreateMap<UserProfile, UserProfileContract>(
            (source, mapper) => new UserProfileContract()
            {
                Id = source.Id,
                Login = source.Login,
                DisplayName = source.DisplayName,
                AvatarUrl =  source.AvatarUrl,
                IsActive =  source.IsActive,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
            });
    }
}