using boltalka.Application.Abstractions.Mappers;
using boltalka.Contracts.Models.Call;
using boltalka.Contracts.Models.Chat;
using boltalka.Contracts.Models.User;

namespace boltalka.WebApi.Mappers.Call;

public class CallFromContractMapper : MappingProfile
{
    public CallFromContractMapper()
    {
        CreateMap<Application.Models.Call.Call, CallContract>(
            (source, mapper) => new CallContract()
            {
                Id = source.Id,
                StartedAt =  source.StartedAt,
                EndedAt =  source.EndedAt,
                Status = (CallStatus)source.Status,
                Type = (CallType)source.Type,
                ChatId = source.ChatId,
                InitiatorId = source.InitiatorId,
                Chat = mapper.Map<ChatContract>(source.Chat),
                Initiator = mapper.Map<UserContract>(source.Initiator),
            });
    }
}