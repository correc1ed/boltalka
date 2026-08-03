using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Chat;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;

namespace boltalka.Application.UseCases.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ChatService(
        IChatRepository chatRepository,
        IUserRepository userRepository,
        IMapper mapper,
        INotificationService notificationService)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _notificationService = notificationService;
    }
    
    public async Task<Result<ChatInfo>> CreatePrivateChatAsync(Guid creatorUserId, Guid otherUserId, CancellationToken cancellationToken)
    {
        if (creatorUserId == otherUserId)
            return Result<ChatInfo>.Failure("Нельзя создать чат с самим собой.");

        // Проверяем существование второго пользователя
        var otherUser = await _userRepository.GetByIdAsync(otherUserId, cancellationToken);
        if (otherUser is null)
            return Result<ChatInfo>.Failure("Пользователь не найден.");

        // Проверяем, нет ли уже приватного чата между этими пользователями
        // (Для этого можно было бы добавить метод в репозиторий, но пока сделаем через существующие)
        var existingChats = await _chatRepository.GetUserChatsAsync(creatorUserId, 0, int.MaxValue, cancellationToken);
        var existingPrivateChat = existingChats
            .FirstOrDefault(c => c.Type == ChatType.Private
                                 && c.Members.Any(m => m.UserId == otherUserId));

        if (existingPrivateChat is not null)
            return Result<ChatInfo>.Success(_mapper.Map<ChatInfo>(existingPrivateChat));

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = ChatType.Private,
            Name = null,
            CreatedAt = DateTime.UtcNow
        };

        var member1 = new ChatMember { UserId = creatorUserId, ChatId = chat.Id, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow };
        var member2 = new ChatMember { UserId = otherUserId, ChatId = chat.Id, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow };

        chat.Members = new List<ChatMember> { member1, member2 };

        await _chatRepository.AddAsync(chat, cancellationToken);

        // Уведомление другому пользователю (опционально)
         await _notificationService.NotifySystemMessageAsync(chat.Id, "Чат создан", cancellationToken);

        return Result<ChatInfo>.Success(_mapper.Map<ChatInfo>(chat));
    }

    public async Task<Result<ChatInfo>> CreateGroupChatAsync(Guid creatorUserId, string name, IEnumerable<Guid> memberIds, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<ChatInfo>.Failure("Название чата не может быть пустым.");

        var uniqueMemberIds = new HashSet<Guid>(memberIds) { creatorUserId }; // создатель всегда участник

        if (uniqueMemberIds.Count < 2)
            return Result<ChatInfo>.Failure("Недостаточно участников для группового чата.");

        // Проверяем, что все пользователи существуют
        foreach (var userId in uniqueMemberIds)
        {
            if (await _userRepository.GetByIdAsync(userId, cancellationToken) is null)
                return Result<ChatInfo>.Failure($"Пользователь {userId} не найден.");
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = ChatType.Group,
            CreatedAt = DateTime.UtcNow,
            Members = uniqueMemberIds.Select(uid => new ChatMember
            {
                UserId = uid,
                ChatId = Guid.Empty, // будет установлен после добавления чата
                Role = uid == creatorUserId ? MemberRole.Admin : MemberRole.Member,
                JoinedAt = DateTime.UtcNow
            }).ToList()
        };

        foreach (var member in chat.Members)
            member.ChatId = chat.Id;

        await _chatRepository.AddAsync(chat, cancellationToken);

        // Уведомляем всех участников о создании группового чата
        await _notificationService.NotifySystemMessageAsync(chat.Id, $"Групповой чат \"{name}\" создан.", cancellationToken);
        foreach (var member in chat.Members)
            await _notificationService.NotifyMemberAddedAsync(chat.Id, member.UserId, cancellationToken);

        return Result<ChatInfo>.Success(_mapper.Map<ChatInfo>(chat));
    }

    public async Task<Result<ChatInfo>> GetChatAsync(Guid chatId, Guid requestUserId, CancellationToken cancellationToken)
    {
        if (!await _chatRepository.IsUserInChatAsync(chatId, requestUserId, cancellationToken))
            return Result<ChatInfo>.Failure("Вы не состоите в этом чате.");

        var chat = await _chatRepository.GetChatWithMembersAsync(chatId, cancellationToken);
        
        if (chat is null)
            return Result<ChatInfo>.Failure("Чат не найден.");

        return Result<ChatInfo>.Success(_mapper.Map<ChatInfo>(chat));
    }

    public async Task<Result<IEnumerable<ChatListItem>>> GetUserChatsAsync(Guid userId, int skip, int take, CancellationToken cancellationToken)
    {
        var chats = await _chatRepository.GetUserChatsAsync(userId, skip, take, cancellationToken);
        return Result<IEnumerable<ChatListItem>>.Success(_mapper.Map<IEnumerable<ChatListItem>>(chats));
    }

    public async Task<Result> AddMemberAsync(Guid chatId, Guid requesterId, Guid newMemberId, CancellationToken ct)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, ct);
        if (chat is null || chat.Type != ChatType.Group)
            return Result.Failure(chat is null ? "Чат не найден." : "Добавление участников возможно только в групповых чатах.");

        var requesterRole = await _chatRepository.GetMemberRoleAsync(chatId, requesterId, ct);
        if (requesterRole != MemberRole.Admin)
            return Result.Failure("Недостаточно прав для добавления участников.");

        if (await _chatRepository.IsUserInChatAsync(chatId, newMemberId, ct))
            return Result.Failure("Пользователь уже в чате.");

        if (await _userRepository.GetByIdAsync(newMemberId, ct) is null)
            return Result.Failure("Пользователь не найден.");

        await _chatRepository.AddMemberToChatAsync(chatId, newMemberId, MemberRole.Member, ct);

        await _notificationService.NotifyMemberAddedAsync(chatId, newMemberId, ct);
        return Result.Success();
    }

    public async Task<Result> RemoveMemberAsync(Guid chatId, Guid requesterId, Guid targetUserId, CancellationToken ct)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, ct);
        if (chat is null)
            return Result.Failure("Чат не найден.");
        if (chat.Type != ChatType.Group)
            return Result.Failure("В личном чате нельзя удалить участника.");

        var isSelfRemoval = requesterId == targetUserId;
        if (!isSelfRemoval)
        {
            var requesterRole = await _chatRepository.GetMemberRoleAsync(chatId, requesterId, ct);
            if (requesterRole != MemberRole.Admin)
                return Result.Failure("Недостаточно прав для удаления участника.");
        }

        if (!await _chatRepository.IsUserInChatAsync(chatId, targetUserId, ct))
            return Result.Failure("Участник не найден в чате.");

        await _chatRepository.RemoveMemberFromChatAsync(chatId, targetUserId, ct);

        await _notificationService.NotifyMemberRemovedAsync(chatId, targetUserId, ct);
        return Result.Success();
    }

    public async Task<Result> ChangeMemberRoleAsync(Guid chatId, Guid requesterId, Guid targetUserId, MemberRole newRole, CancellationToken ct)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, ct);
        if (chat is null || chat.Type != ChatType.Group)
            return Result.Failure(chat is null ? "Чат не найден." : "Роли можно менять только в групповых чатах.");

        var requesterRole = await _chatRepository.GetMemberRoleAsync(chatId, requesterId, ct);
        if (requesterRole != MemberRole.Admin)
            return Result.Failure("Недостаточно прав для изменения роли.");

        var targetExists = await _chatRepository.IsUserInChatAsync(chatId, targetUserId, ct);
        if (!targetExists)
            return Result.Failure("Участник не найден в чате.");

        await _chatRepository.UpdateMemberRoleAsync(chatId, targetUserId, newRole, ct);

        await _notificationService.NotifySystemMessageAsync(chatId, $"Роль пользователя изменена на {newRole}.", ct);
        return Result.Success();
    }

    public async Task<Result> UpdateChatNameAsync(Guid chatId, Guid userId, string newName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure("Название не может быть пустым.");

        var chat = await _chatRepository.GetChatWithMembersAsync(chatId, cancellationToken);
        if (chat is null)
            return Result.Failure("Чат не найден.");

        if (chat.Type != ChatType.Group)
            return Result.Failure("Нельзя изменить название личного чата.");

        var requester = chat.Members.FirstOrDefault(m => m.UserId == userId);
        if (requester is null || await _chatRepository.GetMemberRoleAsync(chatId, userId, cancellationToken) != MemberRole.Admin)
            return Result.Failure("Недостаточно прав для изменения названия.");

        chat.Name = newName;
        await _chatRepository.UpdateAsync(chat, cancellationToken);

        await _notificationService.NotifySystemMessageAsync(chatId, $"Название чата изменено на \"{newName}\".", cancellationToken);
        return Result.Success();
    }
}