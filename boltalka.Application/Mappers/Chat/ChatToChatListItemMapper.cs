using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Enums.Message;

namespace boltalka.Application.Mappers.Chat;

public class ChatToChatListItemMapper : MappingProfile
{
    public ChatToChatListItemMapper()
    {
        CreateMap<Application.Models.Chat.Chat, Application.Models.Chat.ChatListItem>(
            (source, mapper) => new Application.Models.Chat.ChatListItem
            {
                ChatId = source.Id,
                ChatName = source.Name,
                Type = source.Type,
                LastMessage = source.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault(),
                UnreadCount = source.Messages.Count(message => message.Status != MessageStatus.Read),
            });
    }
}