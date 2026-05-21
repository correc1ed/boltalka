using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Models.Media;

namespace boltalka.Infrastructure.Database.Repositories;

public class MediaRepository : BaseRepository<Media>, IMediaRepository
{
    private readonly IMapper _mapper;
    
    public MediaRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext)
    {
        _mapper = mapper;
    }
}