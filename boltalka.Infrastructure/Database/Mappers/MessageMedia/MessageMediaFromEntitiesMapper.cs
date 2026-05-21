using boltalka.Application.Abstractions.Mappers;

namespace boltalka.Infrastructure.Database.Mappers.MessageMedia;

public class MessageMediaFromEntitiesMapper : MappingProfile
{
    public MessageMediaFromEntitiesMapper()
    {
        CreateMap<Entities.MessageMediaEntity, Application.Models.MessageMedia.MessageMedia>(
            (source, mapper) => new Application.Models.MessageMedia.MessageMedia
            {
                MessageId = source.MessageId,
                MediaId = source.MediaId,
                SortOrder = source.SortOrder,
                Message = mapper.Map<Application.Models.Message.Message>(source.Message),
                Media = mapper.Map<Application.Models.Media.Media>(source.Media),
            });
    }
}