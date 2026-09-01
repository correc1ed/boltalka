using boltalka.Application.Abstractions.Mappers;
using boltalka.Contracts.Models.Media;

namespace boltalka.WebApi.Mappers.Media;

public class MediaFromContractMapper : MappingProfile
{
    public MediaFromContractMapper()
    {
        CreateMap<MediaContract, Application.Models.Media.Media>(
            (source, mapper) => new Application.Models.Media.Media()
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                FileName = source.FileName,
                ContentType = source.ContentType,
                SizeBytes = source.SizeBytes,
                StoragePath =  source.StoragePath,
                UploadedByUserId =  source.UploadedByUserId,
                UploadedBy = mapper.Map<Application.Models.User.User>(source.UploadedBy)
            });
    }
}