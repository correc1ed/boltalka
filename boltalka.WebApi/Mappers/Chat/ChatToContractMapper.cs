using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.Chat;
using boltalka.Contracts.Models.Chat;
using boltalka.Contracts.Models.Message;
using boltalka.Contracts.Models.User;

namespace boltalka.WebApi.Mappers.Chat;

public class ChatToContractMapper : MappingProfile
{
    public ChatToContractMapper()
    {
        CreateMap<ChatListItem, ChatListItemContract>(
            (source, mapper) => new ChatListItemContract()
            {
                ChatId =  source.ChatId,
                ChatName =  source.ChatName,
                Type = (ChatType)source.Type,
                LastMessage = mapper.Map<MessageContract>(source.LastMessage),
                UnreadCount = source.UnreadCount,
            });
        
        CreateMap<ChatInfo, ChatInfoContract>(
            (source, mapper) => new ChatInfoContract()
            {
                Id = source.Id,
                Name = source.Name,
                Members = mapper.Map<ICollection<UserProfileContract>>(source.Members),
                LastMessage =  mapper.Map<MessageContract>(source.LastMessage),
                Type = (ChatType)source.Type,
                CreatedAt =  source.CreatedAt,
            });
    }
}