using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.Message;
using boltalka.Contracts.Models.Message;

namespace boltalka.WebApi.Mappers.Message;

public class MessageFromContractMapper : MappingProfile
{
    public MessageFromContractMapper()
    {
        CreateMap<SendMessageContract, SendMessage>(
            (source, mapper) => new SendMessage()
            {
                ChatId =  source.ChatId,
                Text =  source.Text,
                MediaIds =  source.MediaIds,
            });
    }
}