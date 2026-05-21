using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Mappers.Chat;

public class ChatFromEntitiesMapper : MappingProfile
{
    public ChatFromEntitiesMapper()
    {
        CreateMap<Entities.ChatEntity, Application.Models.Chat.Chat>(
            (source, mapper) => new Application.Models.Chat.Chat
            {
                Id = source.Id,
                CreatedAt  = source.CreatedAt,
                UpdatedAt  = source.UpdatedAt,
                Name = source.Name,
                Type = (Application.Enums.Chat.ChatType)source.Type,
                Members = mapper.Map<ICollection<Application.Models.ChatMember.ChatMember>>(source.Members),
                Messages = mapper.Map<ICollection<Application.Models.Message.Message>>(source.Messages),
                Calls = mapper.Map<ICollection<Application.Models.Call.Call>>(source.Calls),
            });
    }
}