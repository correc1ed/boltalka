using boltalka.Application.Abstractions.Mappers;
using boltalka.Contracts.Models.Media;
using boltalka.Contracts.Models.User;

namespace boltalka.WebApi.Mappers.Media;

public class MediaToContractMapper : MappingProfile
{
    public MediaToContractMapper()
    {
        CreateMap<Application.Models.Media.Media, MediaContract>(
            (source, mapper) => new MediaContract()
            {
                Id = source.Id,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                FileName = source.FileName,
                ContentType = source.ContentType,
                SizeBytes = source.SizeBytes,
                StoragePath =  source.StoragePath,
                UploadedByUserId =  source.UploadedByUserId,
                UploadedBy = mapper.Map<UserContract>(source.UploadedBy)
            });
    }
}