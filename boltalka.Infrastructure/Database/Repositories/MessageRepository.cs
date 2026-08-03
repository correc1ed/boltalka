using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Enums.Message;
using boltalka.Application.Models.Message;
using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class MessageRepository : BaseRepository<MessageEntity, Message>, IMessageRepository
{
    public MessageRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
        
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync(
        Guid chatId, int skip, int take, CancellationToken cancellationToken, DateTime? before = null)
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
            .ToListAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<Message>>(resultMessages);
    }

    public async Task<Message?> GetLastMessageAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var resultMessage = await _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        
        return _mapper.Map<Message>(resultMessage);
    }

    public async Task UpdateStatusAsync(Guid messageId, MessageStatus status, CancellationToken cancellationToken)
    {
        var message = await _dbContext.Messages.FindAsync(messageId, cancellationToken);
        if (message is not null)
        {
            message.Status = (Enum.MessageStatus)status;
            _dbContext.Messages.Update(message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Message?> GetMessageWithMediaAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var messageResult = await _dbContext.Messages
            .Include(m => m.MessageMediaLinks)
            .ThenInclude(mm => mm.Media)
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        
        return _mapper.Map<Message>(messageResult);
    }
}