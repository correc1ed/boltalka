using boltalka.Application.Abstractions.Mappers;

namespace boltalka.Application.Mappers.User;

public class UserToUserProfileMapper : MappingProfile
{
    public UserToUserProfileMapper()
    {
        CreateMap<Application.Models.User.User, Application.Models.User.UserProfile>(
        (source, _) => new Application.Models.User.UserProfile
        {
            Id = source.Id,
            Login = source.Login,
            DisplayName = source.DisplayName,
            AvatarUrl = source.Avatar?.StoragePath,
            IsActive = source.IsActive,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
        });
    }
}