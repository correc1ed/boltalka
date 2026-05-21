using boltalka.Application.Abstractions.Mappers;

namespace boltalka.Infrastructure.Database.Mappers.Media;

public class MediaFromEntitiesMapper : MappingProfile
{
    public MediaFromEntitiesMapper()
    {
        CreateMap<Entities.MediaEntity, Application.Models.Media.Media>(
            (source, mapper) => new Application.Models.Media.Media
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                FileName = source.FileName,
                ContentType = source.ContentType,
                SizeBytes = source.SizeBytes,
                StoragePath = source.StoragePath,
                UploadedByUserId = source.UploadedByUserId,
                UploadedBy = mapper.Map<Application.Models.User.User>(source.UploadedBy)
            });
    }
}