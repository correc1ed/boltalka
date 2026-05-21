using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Enums.Message;

namespace boltalka.Infrastructure.Database.Mappers.Message;

public class MessageFromEntitiesMapper : MappingProfile
{
    public MessageFromEntitiesMapper()
    {
        CreateMap<Entities.MessageEntity, Application.Models.Message.Message>(
            (source, mapper) => new Application.Models.Message.Message
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                ChatId  = source.ChatId,
                SenderId = source.SenderId,
                Text = source.Text,
                AttachmentUrl =  source.AttachmentUrl,
                Status = (MessageStatus)source.Status,
                Chat = mapper.Map<Application.Models.Chat.Chat>(source.Chat),
                Sender = mapper.Map<Application.Models.User.User>(source.Sender),
                MessageMediaLinks = mapper.Map<ICollection<Application.Models.MessageMedia.MessageMedia>>(source.MessageMediaLinks),
            });
    }
}