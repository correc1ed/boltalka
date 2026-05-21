using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Models.Chat;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class ChatRepository : BaseRepository<Chat>, IChatRepository
{
    private readonly IMapper _mapper;
    
    public ChatRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<Chat?> GetChatWithMembersAsync(Guid chatId)
    {
        var resultChat = await _dbContext.Chats
            .Include(c => c.Members)
            .ThenInclude(cm => cm.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == chatId);
        
        return _mapper.Map<Chat>(resultChat);
    }

    public async Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId, int skip, int take)
    {
        var chatIds = await _dbContext.ChatMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.ChatId)
            .ToListAsync();

        var resultChats = await _dbContext.Chats
            .Where(c => chatIds.Contains(c.Id))
            .OrderByDescending(c => c.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.CreatedAt)
                .FirstOrDefault())
            .Skip(skip)
            .Take(take)
            .Include(c => c.Members)
            .ThenInclude(cm => cm.User)
            .AsNoTracking()
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<Chat>>(resultChats);
    }

    public async Task<bool> IsUserInChatAsync(Guid chatId, Guid userId)
    {
        return await _dbContext.ChatMembers
            .AnyAsync(cm => cm.ChatId == chatId && cm.UserId == userId);
    }
}