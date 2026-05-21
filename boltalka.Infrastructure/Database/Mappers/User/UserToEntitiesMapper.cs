using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Mappers.User;

public class UserToEntitiesMapper : MappingProfile
{
    public UserToEntitiesMapper()
    {
        CreateMap<Application.Models.User.User, Entities.UserEntity>(
            (source, mapper) => new Entities.UserEntity
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                Login = source.Login,
                PasswordHash =  source.PasswordHash,
                DisplayName = source.DisplayName,
                AvatarMediaId  = source.AvatarMediaId,
                Avatar = mapper.Map<MediaEntity>(source.Avatar),
                IsActive = source.IsActive,
                ChatMembers = mapper.Map<ICollection<ChatMemberEntity>>(source.ChatMembers),
                Messages = mapper.Map<ICollection<MessageEntity>>(source.Messages),
                Calls =  mapper.Map<ICollection<CallEntity>>(source.Calls),
            });
    }
}