using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Enums.ChatMember;

namespace boltalka.Infrastructure.Database.Mappers.ChatMember;

public class ChatMemberFromEntitiesMapper : MappingProfile
{
    public ChatMemberFromEntitiesMapper()
    {
        CreateMap<Entities.ChatMemberEntity, Application.Models.ChatMember.ChatMember>(
            (source, mapper) => new Application.Models.ChatMember.ChatMember
            {
                UserId = source.UserId,
                ChatId = source.ChatId,
                Role = (MemberRole)source.Role,
                JoinedAt = source.JoinedAt,
                User =  mapper.Map<Application.Models.User.User>(source.User),
                Chat = mapper.Map<Application.Models.Chat.Chat>(source.Chat),
            });
    }
}