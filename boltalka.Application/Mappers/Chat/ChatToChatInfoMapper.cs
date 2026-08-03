using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.User;

namespace boltalka.Application.Mappers.Chat;

public class ChatToChatInfoMapper : MappingProfile
{
    public ChatToChatInfoMapper()
    {
        CreateMap<Application.Models.Chat.Chat, Application.Models.Chat.ChatInfo>(
            (source, mapper) => new Application.Models.Chat.ChatInfo
            {
                Id = source.Id,
                Name = source.Name,
                Type = source.Type,
                CreatedAt = source.CreatedAt,
                Members = source.Members.Select(m => mapper.Map<UserProfile>(m.User)).ToList(),
                LastMessage = source.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault(),
            });
    }
}