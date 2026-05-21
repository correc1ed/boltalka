using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;
using boltalka.Infrastructure.Database.Enum;

namespace boltalka.Infrastructure.Database.Mappers.Message;

public class MessageToEntitiesMapper : MappingProfile
{
    public MessageToEntitiesMapper()
    {
        CreateMap<Application.Models.Message.Message, Entities.MessageEntity>(
            (source, mapper) => new Entities.MessageEntity
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                ChatId  = source.ChatId,
                SenderId = source.SenderId,
                Text = source.Text,
                AttachmentUrl =  source.AttachmentUrl,
                Status = (MessageStatus)source.Status,
                Chat = mapper.Map<ChatEntity>(source.Chat),
                Sender = mapper.Map<UserEntity>(source.Sender),
                MessageMediaLinks = mapper.Map<ICollection<MessageMediaEntity>>(source.MessageMediaLinks),
            });
    }
}