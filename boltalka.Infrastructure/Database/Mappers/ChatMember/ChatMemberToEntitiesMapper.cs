using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;
using boltalka.Infrastructure.Database.Enum;

namespace boltalka.Infrastructure.Database.Mappers.ChatMember;

public class ChatMemberToEntitiesMapper : MappingProfile
{
    public ChatMemberToEntitiesMapper()
    {
        CreateMap<Application.Models.ChatMember.ChatMember, Entities.ChatMemberEntity>(
            (source, mapper) => new Entities.ChatMemberEntity
            {
                UserId = source.UserId,
                ChatId = source.ChatId,
                Role = (MemberRole)source.Role,
                JoinedAt = source.JoinedAt,
                User =  mapper.Map<UserEntity>(source.User),
                Chat = mapper.Map<ChatEntity>(source.Chat),
            });
    }
}