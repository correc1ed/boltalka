using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;
using boltalka.Application.Models.User;
using boltalka.Tests.Infrastructure;
using boltalka.Contracts.Models.Chat;
using boltalka.Infrastructure.Database;
using boltalka.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.WebApi;

public class ChatsControllerTests : TestBase
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly ChatsController _controller;
    private readonly ServiceDbContext _dbContext;

    public ChatsControllerTests()
    {
        var chatService = ServiceProvider.GetRequiredService<IChatService>();
        _chatRepository = ServiceProvider.GetRequiredService<IChatRepository>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        _controller = new ChatsController(chatService, mapper);
        _dbContext = ServiceProvider.GetRequiredService<ServiceDbContext>();
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

    private async Task<(Guid user1Id, Guid user2Id, Guid chatId)> CreatePrivateChatAsync(string login1, string login2)
    {
        var user1Id = await CreateUserAsync(login1);
        var user2Id = await CreateUserAsync(login2);
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = (Application.Enums.Chat.ChatType)ChatType.Private,
            CreatedAt = DateTime.UtcNow,
            Members = new List<ChatMember>
            {
                new ChatMember { UserId = user1Id, ChatId = Guid.Empty, Role = (Application.Enums.ChatMember.MemberRole)MemberRole.Member, JoinedAt = DateTime.UtcNow },
                new ChatMember { UserId = user2Id, ChatId = Guid.Empty, Role = (Application.Enums.ChatMember.MemberRole)MemberRole.Member, JoinedAt = DateTime.UtcNow }
            }
        };
        foreach (var m in chat.Members) m.ChatId = chat.Id;
        await _chatRepository.AddAsync(chat, CancellationToken.None);
        return (user1Id, user2Id, chat.Id);
    }

    private async Task<Guid> CreateGroupChatAsync(Guid adminId, string name, params Guid[] memberIds)
    {
        var allMembers = new List<Guid> { adminId };
        allMembers.AddRange(memberIds);
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = (Application.Enums.Chat.ChatType)ChatType.Group,
            CreatedAt = DateTime.UtcNow,
            Members = allMembers.Select(uid => new ChatMember
            {
                UserId = uid,
                ChatId = Guid.Empty,
                Role = uid == adminId ? (Application.Enums.ChatMember.MemberRole)MemberRole.Admin : (Application.Enums.ChatMember.MemberRole)MemberRole.Member,
                JoinedAt = DateTime.UtcNow
            }).ToList()
        };
        foreach (var m in chat.Members) m.ChatId = chat.Id;
        await _chatRepository.AddAsync(chat, CancellationToken.None);
        return chat.Id;
    }

    // ==================== CreatePrivateChat ====================

    [Fact]
    public async Task CreatePrivateChat_Valid_ReturnsOkWithChat()
    {
        var user1Id = await CreateUserAsync("user1");
        var user2Id = await CreateUserAsync("user2");
        SetupAuthenticatedUser(user1Id);

        var contract = new CreatePrivateChatContract { OtherUserId = user2Id };
        var result = await _controller.CreatePrivateChatAsync(contract, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var chatInfo = Assert.IsType<ChatInfoContract>(okResult.Value);
        Assert.Equal(nameof(ChatType.Private), chatInfo.Type.ToString());
    }

    [Fact]
    public async Task CreatePrivateChat_SameUser_ReturnsBadRequest()
    {
        var userId = await CreateUserAsync("user");
        SetupAuthenticatedUser(userId);
        var contract = new CreatePrivateChatContract { OtherUserId = userId };

        var result = await _controller.CreatePrivateChatAsync(contract, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreatePrivateChat_NonExistingUser_ReturnsBadRequest()
    {
        var userId = await CreateUserAsync("user");
        SetupAuthenticatedUser(userId);
        var contract = new CreatePrivateChatContract { OtherUserId = Guid.NewGuid() };

        var result = await _controller.CreatePrivateChatAsync(contract, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== CreateGroupChat ====================

    [Fact]
    public async Task CreateGroupChat_Valid_ReturnsOkWithChat()
    {
        var adminId = await CreateUserAsync("admin");
        var member1Id = await CreateUserAsync("member1");
        var member2Id = await CreateUserAsync("member2");
        SetupAuthenticatedUser(adminId);

        var contract = new CreateGroupChatContract
        {
            Name = "Test Group",
            MemberIds = [ member1Id, member2Id]
        };
        var result = await _controller.CreateGroupChatAsync(contract, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var chatInfo = Assert.IsType<ChatInfoContract>(okResult.Value);
        Assert.Equal("Test Group", chatInfo.Name);
        Assert.Equal(nameof(ChatType.Group), chatInfo.Type.ToString());

        var chatId = chatInfo.Id;
        var members = await _dbContext.ChatMembers.Where(cm => cm.ChatId == chatId).ToListAsync();
        Assert.Equal(3, members.Count);
        Assert.Contains(members, m => m.UserId == adminId && m.Role == (boltalka.Infrastructure.Database.Enum.MemberRole)MemberRole.Admin);
    }

    [Fact]
    public async Task CreateGroupChat_EmptyName_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        SetupAuthenticatedUser(adminId);
        var contract = new CreateGroupChatContract { Name = "", MemberIds = [Guid.NewGuid()] };

        var result = await _controller.CreateGroupChatAsync(contract, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateGroupChat_NotEnoughMembers_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        SetupAuthenticatedUser(adminId);
        var contract = new CreateGroupChatContract { Name = "Solo", MemberIds = new List<Guid>() };

        var result = await _controller.CreateGroupChatAsync(contract, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateGroupChat_NonExistingMember_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        SetupAuthenticatedUser(adminId);
        var contract = new CreateGroupChatContract
        {
            Name = "Invalid",
            MemberIds = [Guid.NewGuid()]
        };

        var result = await _controller.CreateGroupChatAsync(contract, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== GetChat ====================

    [Fact]
    public async Task GetChat_UserIsMember_ReturnsOkWithChat()
    {
        var (user1Id, _, chatId) = await CreatePrivateChatAsync("user1", "user2");
        SetupAuthenticatedUser(user1Id);

        var result = await _controller.GetChatAsync(chatId, CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var chatInfo = Assert.IsType<ChatInfoContract>(okResult.Value);
        Assert.Equal(chatId, chatInfo.Id);
    }

    [Fact]
    public async Task GetChat_UserNotMember_ReturnsBadRequest()
    {
        var (_, _, chatId) = await CreatePrivateChatAsync("user1", "user2");
        var outsiderId = await CreateUserAsync("outsider");
        SetupAuthenticatedUser(outsiderId);

        var result = await _controller.GetChatAsync(chatId, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== GetMyChats ====================

    [Fact]
    public async Task GetMyChats_UserHasChats_ReturnsOkWithList()
    {
        // Создаём всех пользователей один раз
        var user1Id = await CreateUserAsync("user1");
        var user2Id = await CreateUserAsync("user2");
        var user3Id = await CreateUserAsync("user3");

        // Создаём чаты с уже существующими пользователями
        _ = await CreatePrivateChatWithIdsAsync(user1Id, user2Id);
        _ = await CreatePrivateChatWithIdsAsync(user1Id, user3Id);

        SetupAuthenticatedUser(user1Id);

        var result = await _controller.GetMyChatsAsync(skip: 0, take: 10, cancellationToken: CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var chats = Assert.IsAssignableFrom<IEnumerable<ChatListItemContract>>(okResult.Value);
        Assert.Equal(2, chats.Count());
    }

    // ==================== AddMember ====================

    [Fact]
    public async Task AddMember_AdminCanAdd_ReturnsOk()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var newMemberId = await CreateUserAsync("NewMember");
        var chatId = await CreateGroupChatAsync(adminId, "Group", memberId);
        SetupAuthenticatedUser(adminId);

        var result = await _controller.AddMemberAsync(chatId, newMemberId, CancellationToken.None);
        Assert.IsType<OkResult>(result);

        var exists = await _dbContext.ChatMembers.AnyAsync(cm => cm.ChatId == chatId && cm.UserId == newMemberId);
        Assert.True(exists);
    }

    [Fact]
    public async Task AddMember_NotAdmin_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var newMemberId = await CreateUserAsync("NewMember");
        var chatId = await CreateGroupChatAsync(adminId, "Group", memberId);
        SetupAuthenticatedUser(memberId);

        var result = await _controller.AddMemberAsync(chatId, newMemberId, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddMember_AlreadyInChat_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var chatId = await CreateGroupChatAsync(adminId, "Group", memberId);
        SetupAuthenticatedUser(adminId);

        var result = await _controller.AddMemberAsync(chatId, memberId, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== RemoveMember ====================

    [Fact]
    public async Task RemoveMember_AdminRemovesMember_ReturnsOk()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var chatId = await CreateGroupChatAsync(adminId, "Group", memberId);
        SetupAuthenticatedUser(adminId);

        var result = await _controller.RemoveMemberAsync(chatId, memberId, CancellationToken.None);
        Assert.IsType<OkResult>(result);

        var exists = await _dbContext.ChatMembers.AnyAsync(cm => cm.ChatId == chatId && cm.UserId == memberId);
        Assert.False(exists);
    }

    [Fact]
    public async Task RemoveMember_SelfRemoval_ReturnsOk()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var chatId = await CreateGroupChatAsync(adminId, "Group", memberId);
        SetupAuthenticatedUser(memberId);

        var result = await _controller.RemoveMemberAsync(chatId, memberId, CancellationToken.None);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task RemoveMember_NotAdminAndNotSelf_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        var member1Id = await CreateUserAsync("member1");
        var member2Id = await CreateUserAsync("member2");
        var chatId = await CreateGroupChatAsync(adminId, "Group", member1Id, member2Id);
        SetupAuthenticatedUser(member1Id);

        var result = await _controller.RemoveMemberAsync(chatId, member2Id, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== ChangeMemberRole ====================

    [Fact]
    public async Task ChangeMemberRole_AdminChanges_ReturnsOk()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var chatId = await CreateGroupChatAsync(adminId, "Group", memberId);
        SetupAuthenticatedUser(adminId);

        var result = await _controller.ChangeMemberRoleAsync(chatId, memberId, MemberRole.Admin, CancellationToken.None);
        Assert.IsType<OkResult>(result);

        var memberEntity = await _dbContext.ChatMembers.FirstAsync(cm => cm.ChatId == chatId && cm.UserId == memberId);
        Assert.Equal(nameof(MemberRole.Admin), memberEntity.Role.ToString());
    }

    [Fact]
    public async Task ChangeMemberRole_NotAdmin_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        var member1Id = await CreateUserAsync("member1");
        var member2Id = await CreateUserAsync("member2");
        var chatId = await CreateGroupChatAsync(adminId, "Group", member1Id, member2Id);
        SetupAuthenticatedUser(member1Id);

        var result = await _controller.ChangeMemberRoleAsync(chatId, member2Id, MemberRole.Admin, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== UpdateChatName ====================

    [Fact]
    public async Task UpdateChatName_Admin_ReturnsOk()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var chatId = await CreateGroupChatAsync(adminId, "OldName", memberId);
        SetupAuthenticatedUser(adminId);

        var result = await _controller.UpdateChatNameAsync(chatId, "NewName", CancellationToken.None);
        Assert.IsType<OkResult>(result);

        var chatEntity = await _dbContext.Chats.FindAsync(chatId);
        Assert.Equal("NewName", chatEntity!.Name);
    }

    [Fact]
    public async Task UpdateChatName_NotAdmin_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        var memberId = await CreateUserAsync("member");
        var chatId = await CreateGroupChatAsync(adminId, "Group", memberId);
        SetupAuthenticatedUser(memberId);

        var result = await _controller.UpdateChatNameAsync(chatId, "Hacked", CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateChatName_EmptyName_ReturnsBadRequest()
    {
        var adminId = await CreateUserAsync("admin");
        var chatId = await CreateGroupChatAsync(adminId, "Group");
        SetupAuthenticatedUser(adminId);

        var result = await _controller.UpdateChatNameAsync(chatId, "", CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
    private async Task<Guid> CreatePrivateChatWithIdsAsync(Guid user1Id, Guid user2Id)
    {
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Type = (Application.Enums.Chat.ChatType)ChatType.Private,
            CreatedAt = DateTime.UtcNow,
            Members = new List<ChatMember>
            {
                new ChatMember { UserId = user1Id, ChatId = Guid.Empty, Role = (Application.Enums.ChatMember.MemberRole)MemberRole.Member, JoinedAt = DateTime.UtcNow },
                new ChatMember { UserId = user2Id, ChatId = Guid.Empty, Role = (Application.Enums.ChatMember.MemberRole)MemberRole.Member, JoinedAt = DateTime.UtcNow }
            }
        };
        foreach (var m in chat.Members) m.ChatId = chat.Id;
        await _chatRepository.AddAsync(chat, CancellationToken.None);
        return chat.Id;
    }
}