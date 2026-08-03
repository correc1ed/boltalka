using System.Linq;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Call;
using boltalka.Application.Enums.Chat;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models.Call;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;
using boltalka.Application.Models.User;
using boltalka.Application.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.Services;

public class CallServiceTests : TestBase
{
    private readonly ICallService _callService;
    private readonly ICallRepository _callRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;

    public CallServiceTests()
    {
        _callService = ServiceProvider.GetRequiredService<ICallService>();
        _callRepository = ServiceProvider.GetRequiredService<ICallRepository>();
        _chatRepository = ServiceProvider.GetRequiredService<IChatRepository>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ICallRepository, CallRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICallService, CallService>();
    }

    // ==================== StartCallAsync ====================

    [Fact]
    public async Task StartCall_ValidUserAndNoActiveCall_StartsCall()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");

        var result = await _callService.StartCallAsync(chatId, initiatorId, CallType.Audio, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CallStatus.Pending, result.Value.Status);
        Assert.Equal(CallType.Audio, result.Value.Type);
        Assert.Equal(initiatorId, result.Value.InitiatorId);
        Assert.Equal(chatId, result.Value.ChatId);

        // Проверяем через БД
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var call = await db.Calls.FirstOrDefaultAsync(c => c.Id == result.Value.Id);
        Assert.NotNull(call);
        Assert.Equal(CallStatus.Pending, (CallStatus)call.Status);
    }

    [Fact]
    public async Task StartCall_UserNotInChat_ReturnsFailure()
    {
        var outsider = await CreateUserAsync("outsider");
        var (_, chatId) = await CreateChatWithUserAsync("alice", "bob");

        var result = await _callService.StartCallAsync(chatId, outsider, CallType.Audio, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не являетесь участником", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartCall_ActiveCallExists_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        // Создаём активный звонок через репозиторий
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Active,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);

        var result = await _callService.StartCallAsync(chatId, initiatorId, CallType.Audio, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Contains("уже есть активный звонок", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartCall_PendingCallExists_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Pending,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);

        var result = await _callService.StartCallAsync(chatId, initiatorId, CallType.Audio, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Contains("уже есть активный звонок", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== AcceptCallAsync ====================

    [Fact]
    public async Task AcceptCall_PendingCallByParticipant_ActivatesCall()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Pending,
            Type = CallType.Video,
            InitiatorId = initiatorId,
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);

        // Принимает другой участник (получатель)
        var receiverId = await GetOtherUserIdInChat(chatId, initiatorId);

        var result = await _callService.AcceptCallAsync(pendingCall.Id, receiverId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CallStatus.Active, result.Value.Status);

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var call = await db.Calls.FindAsync(pendingCall.Id);
        Assert.Equal(CallStatus.Active, (CallStatus)call!.Status);
    }

    [Fact]
    public async Task AcceptCall_NotPendingStatus_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Active,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        var receiverId = await GetOtherUserIdInChat(chatId, initiatorId);

        var result = await _callService.AcceptCallAsync(activeCall.Id, receiverId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Нельзя принять звонок", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AcceptCall_UserNotInChat_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var outsider = await CreateUserAsync("outsider");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Pending,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);

        var result = await _callService.AcceptCallAsync(pendingCall.Id, outsider, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не являетесь участником", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AcceptCall_NonExistingCall_ReturnsFailure()
    {
        var result = await _callService.AcceptCallAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Contains("не найден", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== EndCallAsync ====================

    [Fact]
    public async Task EndCall_ActiveCallByParticipant_EndsCall()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Active,
            Type = CallType.Video
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);

        var result = await _callService.EndCallAsync(activeCall.Id, initiatorId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CallStatus.Ended, result.Value.Status);
        Assert.NotNull(result.Value.EndedAt);

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var call = await db.Calls.FindAsync(activeCall.Id);
        Assert.Equal(CallStatus.Ended, (CallStatus)call!.Status);
        Assert.NotNull(call.EndedAt);
    }

    [Fact]
    public async Task EndCall_PendingCallByInitiator_EndsCall()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Pending,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);

        var result = await _callService.EndCallAsync(pendingCall.Id, initiatorId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CallStatus.Ended, result.Value.Status);
    }

    [Fact]
    public async Task EndCall_UserNotInChat_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var outsider = await CreateUserAsync("outsider");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Active,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);

        var result = await _callService.EndCallAsync(activeCall.Id, outsider, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не являетесь участником", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EndCall_AlreadyEnded_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var endedCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Ended,
            Type = CallType.Video,
            EndedAt = DateTime.UtcNow
        };
        await _callRepository.AddAsync(endedCall, CancellationToken.None);

        var result = await _callService.EndCallAsync(endedCall.Id, initiatorId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("уже завершён", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== DeclineCallAsync ====================

    [Fact]
    public async Task DeclineCall_PendingCallByReceiver_MarksMissed()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Pending,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);
        var receiverId = await GetOtherUserIdInChat(chatId, initiatorId);

        var result = await _callService.DeclineCallAsync(pendingCall.Id, receiverId, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var call = await db.Calls.FindAsync(pendingCall.Id);
        Assert.Equal(CallStatus.Missed, (CallStatus)call!.Status);
    }

    [Fact]
    public async Task DeclineCall_ByInitiator_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Pending,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);

        var result = await _callService.DeclineCallAsync(pendingCall.Id, initiatorId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Инициатор не может отклонить", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeclineCall_NotPendingStatus_ReturnsFailure()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Active,
            Type = CallType.Video
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        var receiverId = await GetOtherUserIdInChat(chatId, initiatorId);

        var result = await _callService.DeclineCallAsync(activeCall.Id, receiverId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("нельзя отклонить", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== GetActiveCallAsync ====================

    [Fact]
    public async Task GetActiveCall_ActiveExists_ReturnsCallDto()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Active,
            Type = CallType.Audio
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);

        var result = await _callService.GetActiveCallAsync(chatId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(activeCall.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetActiveCall_NoActiveCall_ReturnsNull()
    {
        var (_, chatId) = await CreateChatWithUserAsync("alice", "bob");
        var result = await _callService.GetActiveCallAsync(chatId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    // ==================== GetCallHistoryAsync ====================

    [Fact]
    public async Task GetCallHistory_Member_ReturnsCalls()
    {
        var (initiatorId, chatId) = await CreateChatWithUserAsync("caller", "receiver");
        var call1 = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Ended,
            Type = CallType.Audio,
            EndedAt = DateTime.UtcNow
        };
        var call2 = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = CallStatus.Missed,
            Type = CallType.Video,
            EndedAt = DateTime.UtcNow
        };
        await _callRepository.AddAsync(call1, CancellationToken.None);
        await _callRepository.AddAsync(call2, CancellationToken.None);

        var result = await _callService.GetCallHistoryAsync(chatId, initiatorId, 0, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var list = result.Value.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, c => c.Id == call1.Id);
        Assert.Contains(list, c => c.Id == call2.Id);
    }

    [Fact]
    public async Task GetCallHistory_NotMember_ReturnsFailure()
    {
        var (_, chatId) = await CreateChatWithUserAsync("alice", "bob");
        var outsider = await CreateUserAsync("outsider");

        var result = await _callService.GetCallHistoryAsync(chatId, outsider, 0, 10, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не являетесь участником", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== Вспомогательные методы ====================

    private async Task<Guid> CreateUserAsync(string login)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            DisplayName = login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.AddAsync(user, CancellationToken.None);
        return user.Id;
    }

    private async Task<(Guid userId1, Guid chatId)> CreateChatWithUserAsync(string user1Login, string user2Login)
    {
        var user1Id = await CreateUserAsync(user1Login);
        var user2Id = await CreateUserAsync(user2Login);

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = null,
            Type = ChatType.Private,
            CreatedAt = DateTime.UtcNow,
            Members = new List<ChatMember>
            {
                new ChatMember { UserId = user1Id, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow },
                new ChatMember { UserId = user2Id, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow }
            }
        };
        foreach (var m in chat.Members) m.ChatId = chat.Id;
        await _chatRepository.AddAsync(chat, CancellationToken.None);
        return (user1Id, chat.Id);
    }

    private async Task<Guid> GetOtherUserIdInChat(Guid chatId, Guid excludeUserId)
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var member = await db.ChatMembers
            .Where(cm => cm.ChatId == chatId && cm.UserId != excludeUserId)
            .Select(cm => cm.UserId)
            .FirstOrDefaultAsync();
        return member;
    }
}