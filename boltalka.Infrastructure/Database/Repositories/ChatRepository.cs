using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;
using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class ChatRepository : BaseRepository<ChatEntity, Chat>, IChatRepository
{
    public ChatRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }

    public async Task<Chat?> GetChatWithMembersAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var resultChat = await _dbContext.Chats
            .Include(c => c.Members)
            .ThenInclude(cm => cm.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
        
        return _mapper.Map<Chat>(resultChat);
    }

    public async Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId, int skip, int take, CancellationToken cancellationToken)
    {
        var chatIds = await _dbContext.ChatMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.ChatId)
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<Chat>>(resultChats);
    }

    public async Task<bool> IsUserInChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.ChatMembers
            .AnyAsync(cm => cm.ChatId == chatId && cm.UserId == userId, cancellationToken);
    }

    public async Task<MemberRole?> GetMemberRoleAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ChatMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(cm => cm.ChatId == chatId && cm.UserId == userId, cancellationToken);

        if (member == null)
            return null;
        
        return (MemberRole)member.Role;
    }

    public async Task AddMemberToChatAsync(Guid chatId, Guid userId, MemberRole role, CancellationToken cancellationToken = default)
    {
        var member = new ChatMemberEntity()
        {
            ChatId = chatId,
            UserId = userId,
            Role = (Enum.MemberRole)role,
            JoinedAt = DateTime.UtcNow
        };
        await _dbContext.ChatMembers.AddAsync(member, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMemberRoleAsync(Guid chatId, Guid userId, MemberRole newRole, CancellationToken ct = default)
    {
        var member = await _dbContext.ChatMembers
            .FirstOrDefaultAsync(cm => cm.ChatId == chatId && cm.UserId == userId, ct);
        if (member != null)
        {
            member.Role = (Enum.MemberRole)newRole;
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveMemberFromChatAsync(Guid chatId, Guid userId, CancellationToken ct = default)
    {
        var member = await _dbContext.ChatMembers
            .FirstOrDefaultAsync(cm => cm.ChatId == chatId && cm.UserId == userId, ct);
        if (member != null)
        {
            _dbContext.ChatMembers.Remove(member);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}