using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Models.Media;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Repositories;

public class MediaRepository : BaseRepository<MediaEntity, Media>, IMediaRepository
{
    public MediaRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
        
    }
}