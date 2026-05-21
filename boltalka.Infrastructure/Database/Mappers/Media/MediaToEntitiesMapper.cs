using boltalka.Application.Abstractions.Mappers;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Mappers.Media;

public class MediaToEntitiesMapper : MappingProfile
{
    public MediaToEntitiesMapper()
    {
        CreateMap<Application.Models.Media.Media, Entities.MediaEntity>(
            (source, mapper) => new Entities.MediaEntity
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                FileName = source.FileName,
                ContentType = source.ContentType,
                SizeBytes = source.SizeBytes,
                StoragePath = source.StoragePath,
                UploadedByUserId = source.UploadedByUserId,
                UploadedBy = mapper.Map<UserEntity>(source.UploadedBy)
            });
    }
}