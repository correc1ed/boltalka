using boltalka.Contracts.Models.Chat;
using boltalka.Contracts.Models.Media;
using boltalka.Contracts.Models.Message;
using boltalka.Contracts.Models.User;

namespace boltalka.WebApi.Mappers.Message;

public class MessageToContractMapper : MappingProfile
{
    public MessageToContractMapper()
    {
        CreateMap<Application.Models.Message.Message, MessageInfoContract>(
            (source, _) => new MessageInfoContract()
            {
                Id = source.Id,
                CreatedAt =  source.CreatedAt,
                UpdatedAt =  source.UpdatedAt,
                ChatId = source.ChatId,
                SenderId = source.SenderId,
                SenderName = source.Sender?.DisplayName ?? string.Empty,
                //AttachmentUrl = source.AttachmentUrl,
                Status = (MessageStatus)source.Status,
                Text =  source.Text,
            });
        
        CreateMap<Application.Models.Message.Message, MessageContract>(
            (source, mapper) => new MessageContract()
            {
                Id = source.Id,
                CreatedAt =  source.CreatedAt,
                UpdatedAt =  source.UpdatedAt,
                ChatId = source.ChatId,
                Text =  source.Text,
                SenderId = source.SenderId,
                AttachmentUrl = source.AttachmentUrl,
                Status = (MessageStatusContract)source.Status,
                Chat = mapper.Map<ChatContract>(source.Chat),
                Sender = mapper.Map<UserContract>(source.Sender),
                MessageMediaLinks = mapper.Map<ICollection<MessageMediaContract>>(source.MessageMediaLinks),
            });
        
        
        CreateMap<Application.Models.Message.MessageInfo, MessageInfoContract>(
            (source, mapper) => new MessageInfoContract()
            {
                Id = source.Id,
                CreatedAt =  source.CreatedAt,
                UpdatedAt =  source.UpdatedAt,
                ChatId = source.ChatId,
                SenderId = source.SenderId,
                SenderName = source.SenderName,
                Attachments = mapper.Map<IEnumerable<MediaContract>>(source.Attachments),
                Status = (MessageStatus)source.Status,
                Text =  source.Text,
            });
    }
}