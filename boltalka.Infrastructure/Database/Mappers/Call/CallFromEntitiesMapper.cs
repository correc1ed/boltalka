using boltalka.Application.Abstractions.Mappers;

namespace boltalka.Infrastructure.Database.Mappers.Call;

public class CallFromEntitiesMapper : MappingProfile
{
    public CallFromEntitiesMapper()
    {
        CreateMap<Entities.CallEntity, Application.Models.Call.Call>(
        (source, mapper) => new Application.Models.Call.Call
        {
            Id = source.Id,
            ChatId = source.ChatId,
            InitiatorId = source.InitiatorId,
            StartedAt  = source.StartedAt,
            EndedAt  = source.EndedAt,
            Type = (Application.Enums.Call.CallType)source.Type,
            Status = (Application.Enums.Call.CallStatus)source.Status,
            Chat = mapper.Map<Application.Models.Chat.Chat>(source.Chat),
            Initiator = mapper.Map<Application.Models.User.User>(source.Initiator),
        });
    }
}