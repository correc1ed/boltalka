using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Mappers.User;

public class UserFromEntitiesMapper : MappingProfile
{
    public UserFromEntitiesMapper()
    {
        CreateMap<Entities.UserEntity, Application.Models.User.User>(
            (source, mapper) => new Application.Models.User.User
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                Login = source.Login,
                PasswordHash =  source.PasswordHash,
                DisplayName = source.DisplayName,
                AvatarMediaId  = source.AvatarMediaId,
                Avatar = mapper.Map<Application.Models.Media.Media>(source.Avatar),
                IsActive = source.IsActive,
                ChatMembers = mapper.Map<ICollection<Application.Models.ChatMember.ChatMember>>(source.ChatMembers),
                Messages = mapper.Map<ICollection<Application.Models.Message.Message>>(source.Messages),
                Calls =  mapper.Map<ICollection<Application.Models.Call.Call>>(source.Calls),
            });
    }
}