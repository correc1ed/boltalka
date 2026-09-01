using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.Chat;
using boltalka.Contracts.Models.Chat;
using ChatType = boltalka.Application.Enums.Chat.ChatType;

namespace boltalka.WebApi.Mappers.Chat;

public class ChatFromContractMapper : MappingProfile
{
    public ChatFromContractMapper()
    {
        CreateMap<ChatListItemContract, ChatListItem>(
            (source, mapper) => new ChatListItem()
            {
                ChatId =  source.ChatId,
                ChatName =  source.ChatName,
                Type = (ChatType)source.Type,
                LastMessage = mapper.Map<Application.Models.Message.Message>(source.LastMessage),
                UnreadCount =  source.UnreadCount,
            });
    }
}