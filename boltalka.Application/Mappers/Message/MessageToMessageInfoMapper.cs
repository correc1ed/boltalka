using boltalka.Application.Abstractions.Mappers;

namespace boltalka.Application.Mappers.Message;

public class MessageToMessageInfoMapper : MappingProfile
{
    public MessageToMessageInfoMapper()
    {
        CreateMap<Application.Models.Message.Message, Application.Models.Message.MessageInfo>(
            (source, mapper) => new Application.Models.Message.MessageInfo
            {
                Id = source.Id,
                ChatId = source.ChatId,
                SenderId = source.SenderId,
                SenderName = source.Sender?.DisplayName != null ? source.Sender.DisplayName : string.Empty,
                Text = source.Text,
                Attachments = source.MessageMediaLinks.Select(messageMediaLink => messageMediaLink.Media),
                Status = source.Status,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
            });
    }
}