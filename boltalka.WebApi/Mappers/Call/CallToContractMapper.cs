using boltalka.Application.Abstractions.Mappers;
using boltalka.Contracts.Models.Call;
using boltalka.Contracts.Models.Chat;
using boltalka.Contracts.Models.User;
using CallStatus = boltalka.Application.Enums.Call.CallStatus;
using CallType = boltalka.Application.Enums.Call.CallType;

namespace boltalka.WebApi.Mappers.Call;

public class CallToContractMapper : MappingProfile
{
    public CallToContractMapper()
    {
        CreateMap<CallContract, Application.Models.Call.Call>(
            (source, mapper) => new Application.Models.Call.Call()
            {
                Id = source.Id,
                StartedAt =  source.StartedAt,
                EndedAt =  source.EndedAt,
                Status = (CallStatus)source.Status,
                Type = (CallType)source.Type,
                ChatId = source.ChatId,
                InitiatorId = source.InitiatorId,
                Chat = mapper.Map<Application.Models.Chat.Chat>(source.Chat),
                Initiator = mapper.Map<Application.Models.User.User>(source.Initiator),
            });
    }
}