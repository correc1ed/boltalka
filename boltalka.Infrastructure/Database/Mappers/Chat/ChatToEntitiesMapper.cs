using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;
using boltalka.Infrastructure.Database.Enum;

namespace boltalka.Infrastructure.Database.Mappers.Chat;

public class ChatToEntitiesMapper : MappingProfile
{
    public ChatToEntitiesMapper()
    {
        CreateMap<Application.Models.Chat.Chat, Entities.ChatEntity>(
            (source, mapper) => new Entities.ChatEntity
            {
                Id = source.Id,
                CreatedAt  = source.CreatedAt,
                UpdatedAt  = source.UpdatedAt,
                Name = source.Name,
                Type = (ChatType)source.Type,
                Members = mapper.Map<ICollection<ChatMemberEntity>>(source.Members),
                Messages = mapper.Map<ICollection<MessageEntity>>(source.Messages),
                Calls = mapper.Map<ICollection<CallEntity>>(source.Calls),
            });
    }
}