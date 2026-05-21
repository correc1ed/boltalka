using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;
using boltalka.Infrastructure.Database.Enum;

namespace boltalka.Infrastructure.Database.Mappers.Call;

public class CallToEntitiesMapper : MappingProfile
{
    CallToEntitiesMapper()
    {
        CreateMap<Application.Models.Call.Call, Entities.CallEntity>(
            (source, mapper) => new Entities.CallEntity
            {
                Id = source.Id,
                ChatId = source.ChatId,
                InitiatorId = source.InitiatorId,
                StartedAt  = source.StartedAt,
                EndedAt  = source.EndedAt,
                Type = (CallType)source.Type,
                Status = (CallStatus)source.Status,
                Chat = mapper.Map<ChatEntity>(source.Chat),
                Initiator = mapper.Map<UserEntity>(source.Initiator),
            });
    }
}