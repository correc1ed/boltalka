using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Mappers.MessageMedia;

public class MessageMediaToEntitiesMapper : MappingProfile
{
    public MessageMediaToEntitiesMapper()
    {
        CreateMap<Application.Models.MessageMedia.MessageMedia, Entities.MessageMediaEntity>(
            (source, mapper) => new Entities.MessageMediaEntity
            {
                MessageId = source.MessageId,
                MediaId = source.MediaId,
                SortOrder = source.SortOrder,
                Message = mapper.Map<MessageEntity>(source.Message),
                Media = mapper.Map<MediaEntity>(source.Media),
            });
    }
}