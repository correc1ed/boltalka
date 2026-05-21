using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Enums.Message;
using boltalka.Application.Models.Message;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    private readonly IMapper _mapper;
    
    public MessageRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(
        Guid chatId, int skip, int take, DateTime? before = null)
    {
        var query = _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId);

        if (before.HasValue)
        {
            query = query.Where(m => m.CreatedAt < before.Value);
        }

        var resultMessages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<Message>>(resultMessages);
    }

    public async Task<Message?> GetLastMessageAsync(Guid chatId)
    {
        var resultMessage = await _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
        
        return _mapper.Map<Message>(resultMessage);
    }

    public async Task UpdateStatusAsync(Guid messageId, MessageStatus status)
    {
        var message = await _dbContext.Messages.FindAsync(messageId);
        if (message is not null)
        {
            message.Status = (Enum.MessageStatus)status;
            _dbContext.Messages.Update(message);
        }
    }
}