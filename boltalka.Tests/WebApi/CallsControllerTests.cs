using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Chat;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models.Call;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;
using boltalka.Application.Models.User;
using boltalka.Tests.Infrastructure;
using boltalka.Contracts.Models.Call;
using boltalka.Infrastructure.Database;
using boltalka.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.WebApi;

public class CallsControllerTests : TestBase
{
    private readonly ICallRepository _callRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly CallsController _controller;

    public CallsControllerTests()
    {
        var callService = ServiceProvider.GetRequiredService<ICallService>();
        _callRepository = ServiceProvider.GetRequiredService<ICallRepository>();
        _chatRepository = ServiceProvider.GetRequiredService<IChatRepository>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        _controller = new CallsController(callService, mapper);
    }

    private void SetupAuthenticatedUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

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

    private async Task<(Guid userId1, Guid chatId)> CreateChatWithTwoUsersAsync(string login1, string login2)
    {
        var userId1 = await CreateUserAsync(login1);
        var userId2 = await CreateUserAsync(login2);

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = null,
            Type = ChatType.Private,
            CreatedAt = DateTime.UtcNow,
            Members = new List<ChatMember>
            {
                new ChatMember { UserId = userId1, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow },
                new ChatMember { UserId = userId2, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow }
            }
        };
        foreach (var m in chat.Members) m.ChatId = chat.Id;
        await _chatRepository.AddAsync(chat, CancellationToken.None);
        return (userId1, chat.Id);
    }

    private async Task<Guid> GetOtherUserIdInChatAsync(Guid chatId, Guid excludeUserId)
    {
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var member = await db.ChatMembers
            .Where(cm => cm.ChatId == chatId && cm.UserId != excludeUserId)
            .Select(cm => cm.UserId)
            .FirstOrDefaultAsync();
        return member;
    }

    // ==================== StartCall ====================

    [Fact]
    public async Task StartCall_Valid_ReturnsOkWithCall()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.StartCallAsync(chatId, CallType.Audio, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var callContract = Assert.IsType<CallContract>(okResult.Value); // или CallDto, если используется
        Assert.Equal(chatId, callContract.ChatId);
        Assert.Equal(initiatorId, callContract.InitiatorId);
        Assert.Equal(CallStatus.Pending, callContract.Status);
    }

    [Fact]
    public async Task StartCall_UserNotInChat_ReturnsBadRequest()
    {
        // Arrange
        var (_, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var outsiderId = await CreateUserAsync("outsider");
        SetupAuthenticatedUser(outsiderId);

        // Act
        var result = await _controller.StartCallAsync(chatId, CallType.Audio, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task StartCall_ActiveCallExists_ReturnsBadRequest()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Active,
            Type = (Application.Enums.Call.CallType)CallType.Video
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.StartCallAsync(chatId, CallType.Audio, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== AcceptCall ====================

    [Fact]
    public async Task AcceptCall_PendingCall_ReturnsOk()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var receiverId = await GetOtherUserIdInChatAsync(chatId, initiatorId);
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Pending,
            Type = (Application.Enums.Call.CallType)CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);
        SetupAuthenticatedUser(receiverId);

        // Act
        var result = await _controller.AcceptCallAsync(pendingCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var callContract = Assert.IsType<CallContract>(okResult.Value);
        Assert.Equal(CallStatus.Active, callContract.Status);
    }

    [Fact]
    public async Task AcceptCall_NonPendingStatus_ReturnsBadRequest()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var receiverId = await GetOtherUserIdInChatAsync(chatId, initiatorId);
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Active,
            Type = (Application.Enums.Call.CallType)CallType.Video
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        SetupAuthenticatedUser(receiverId);

        // Act
        var result = await _controller.AcceptCallAsync(activeCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AcceptCall_UserNotInChat_ReturnsBadRequest()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var outsiderId = await CreateUserAsync("outsider");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Pending,
            Type = (Application.Enums.Call.CallType)CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);
        SetupAuthenticatedUser(outsiderId);

        // Act
        var result = await _controller.AcceptCallAsync(pendingCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AcceptCall_NonExistingCall_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("SomeUser");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.AcceptCallAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== EndCall ====================

    [Fact]
    public async Task EndCall_ActiveCall_ReturnsOk()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Active,
            Type = (Application.Enums.Call.CallType)CallType.Video
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.EndCallAsync(activeCall.Id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var callContract = Assert.IsType<CallContract>(okResult.Value);
        Assert.Equal(CallStatus.Ended, callContract.Status);
        Assert.NotNull(callContract.EndedAt);
    }

    [Fact]
    public async Task EndCall_UserNotInChat_ReturnsBadRequest()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var outsiderId = await CreateUserAsync("outsider");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Active,
            Type = (Application.Enums.Call.CallType)CallType.Audio
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        SetupAuthenticatedUser(outsiderId);

        // Act
        var result = await _controller.EndCallAsync(activeCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EndCall_AlreadyEnded_ReturnsBadRequest()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var endedCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Ended,
            Type = (Application.Enums.Call.CallType)CallType.Video,
            EndedAt = DateTime.UtcNow
        };
        await _callRepository.AddAsync(endedCall, CancellationToken.None);
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.EndCallAsync(endedCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== DeclineCall ====================

    [Fact]
    public async Task DeclineCall_PendingCallByReceiver_ReturnsOk()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var receiverId = await GetOtherUserIdInChatAsync(chatId, initiatorId);
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Pending,
            Type = (Application.Enums.Call.CallType)CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);
        SetupAuthenticatedUser(receiverId);

        // Act
        var result = await _controller.DeclineCallAsync(pendingCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeclineCall_ByInitiator_ReturnsBadRequest()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var pendingCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Pending,
            Type = (Application.Enums.Call.CallType)CallType.Audio
        };
        await _callRepository.AddAsync(pendingCall, CancellationToken.None);
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.DeclineCallAsync(pendingCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeclineCall_NotPendingStatus_ReturnsBadRequest()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var receiverId = await GetOtherUserIdInChatAsync(chatId, initiatorId);
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Active,
            Type = (Application.Enums.Call.CallType)CallType.Video
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        SetupAuthenticatedUser(receiverId);

        // Act
        var result = await _controller.DeclineCallAsync(activeCall.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== GetActiveCall ====================

    [Fact]
    public async Task GetActiveCall_ActiveExists_ReturnsOkWithCall()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var activeCall = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Active,
            Type = (Application.Enums.Call.CallType)CallType.Audio
        };
        await _callRepository.AddAsync(activeCall, CancellationToken.None);
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.GetActiveCallAsync(chatId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var callContract = Assert.IsType<CallContract>(okResult.Value);
        Assert.Equal(activeCall.Id, callContract.Id);
    }

    [Fact]
    public async Task GetActiveCall_NoActiveCall_ReturnsOkNull()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.GetActiveCallAsync(chatId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(okResult.Value);
    }

    // ==================== GetCallHistory ====================

    [Fact]
    public async Task GetCallHistory_Member_ReturnsOkWithList()
    {
        // Arrange
        var (initiatorId, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var call1 = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = (Application.Enums.Call.CallStatus)CallStatus.Ended,
            Type = (Application.Enums.Call.CallType)CallType.Audio,
            EndedAt = DateTime.UtcNow
        };
        var call2 = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = (Application.Enums.Call.CallStatus)CallStatus.Missed,
            Type = (Application.Enums.Call.CallType)CallType.Video,
            EndedAt = DateTime.UtcNow
        };
        await _callRepository.AddAsync(call1, CancellationToken.None);
        await _callRepository.AddAsync(call2, CancellationToken.None);
        SetupAuthenticatedUser(initiatorId);

        // Act
        var result = await _controller.GetCallHistoryAsync(chatId, 0, 10, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var calls = Assert.IsAssignableFrom<IEnumerable<CallContract>>(okResult.Value);
        Assert.Equal(2, calls.Count());
    }

    [Fact]
    public async Task GetCallHistory_UserNotInChat_ReturnsBadRequest()
    {
        // Arrange
        var (_, chatId) = await CreateChatWithTwoUsersAsync("caller", "receiver");
        var outsiderId = await CreateUserAsync("outsider");
        SetupAuthenticatedUser(outsiderId);

        // Act
        var result = await _controller.GetCallHistoryAsync(chatId, 0, 10, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}