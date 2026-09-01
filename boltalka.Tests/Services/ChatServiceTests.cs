using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Chat;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models.Message;
using boltalka.Application.Models.User;
using boltalka.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.Services;

public class ChatServiceTests : TestBase
{
    private readonly IChatService _chatService;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;

    public ChatServiceTests()
    {
        _chatService = ServiceProvider.GetRequiredService<IChatService>();
        _chatRepository = ServiceProvider.GetRequiredService<IChatRepository>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatService, ChatService>();
    }

    // ==================== CreatePrivateChatAsync ====================

    [Fact]
    public async Task CreatePrivateChat_ValidUsers_CreatesChat()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");

        var result = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var chat = result.Value;
        Assert.NotNull(chat);
        Assert.Equal(ChatType.Private, chat.Type);
        Assert.Null(chat.Name);

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var chatId = chat.Id;
        var members = await db.ChatMembers.Where(cm => cm.ChatId == chatId).ToListAsync();
        Assert.Equal(2, members.Count);
        Assert.Contains(members, m => m.UserId == user1);
        Assert.Contains(members, m => m.UserId == user2);
    }

    [Fact]
    public async Task CreatePrivateChat_SameUser_ReturnsFailure()
    {
        var user = await CreateUserAsync("user");
        var result = await _chatService.CreatePrivateChatAsync(user, user, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("самим собой", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreatePrivateChat_AlreadyExists_ReturnsExisting()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");

        var firstResult = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);
        var firstChat = firstResult.Value;
        Assert.NotNull(firstChat);

        var secondResult = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        Assert.True(secondResult.IsSuccess);
        var secondChat = secondResult.Value;
        Assert.NotNull(secondChat);

        Assert.Equal(firstChat.Id, secondChat.Id); // тот же чат
    }

    [Fact]
    public async Task CreatePrivateChat_NonExistingUser_ReturnsFailure()
    {
        var user1 = await CreateUserAsync("user1");
        var result = await _chatService.CreatePrivateChatAsync(user1, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("не найден", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== CreateGroupChatAsync ====================

    [Fact]
    public async Task CreateGroupChat_ValidData_CreatesChat()
    {
        var admin = await CreateUserAsync("admin");
        var member1 = await CreateUserAsync("member1");
        var member2 = await CreateUserAsync("member2");

        var result = await _chatService.CreateGroupChatAsync(admin, "Test Group", [member1, member2], CancellationToken.None);

        Assert.True(result.IsSuccess);
        var chat = result.Value;
        Assert.NotNull(chat);
        Assert.Equal("Test Group", chat.Name);
        Assert.Equal(ChatType.Group, chat.Type);

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var chatId = chat.Id;

        var members = await db.ChatMembers
            .Where(cm => cm.ChatId == chatId)
            .ToListAsync();

        Assert.Equal(3, members.Count);
        Assert.Contains(members, m => m.UserId == admin && (MemberRole)m.Role == MemberRole.Admin);
        Assert.Contains(members, m => m.UserId == member1 && (MemberRole)m.Role == MemberRole.Member);
        Assert.Contains(members, m => m.UserId == member2 && (MemberRole)m.Role == MemberRole.Member);
    }

    [Fact]
    public async Task CreateGroupChat_EmptyName_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var memberIds = new List<Guid> { await CreateUserAsync("member") };
        var result = await _chatService.CreateGroupChatAsync(admin, "", memberIds, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("не может быть пустым", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateGroupChat_NotEnoughMembers_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var result = await _chatService.CreateGroupChatAsync(admin, "Solo", new List<Guid>(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("Недостаточно", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateGroupChat_NonExistingUser_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var memberIds = new List<Guid> { Guid.NewGuid() };
        var result = await _chatService.CreateGroupChatAsync(admin, "Invalid", memberIds, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("не найден", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== GetChatAsync ====================

    [Fact]
    public async Task GetChat_Member_ReturnsChatWithMembers()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");
        var createResult = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        Assert.True(createResult.IsSuccess);
        var createdChat = createResult.Value;
        Assert.NotNull(createdChat);

        var result = await _chatService.GetChatAsync(createdChat.Id, user1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var chat = result.Value;
        Assert.NotNull(chat);
        Assert.Equal(createdChat.Id, chat.Id);
        // Members не-nullable, поэтому убираем ?.
        Assert.Equal(2, chat.Members.Count);
    }

    [Fact]
    public async Task GetChat_NotMember_ReturnsFailure()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");
        var outsider = await CreateUserAsync("outsider");
        var chat = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.GetChatAsync(chatValue.Id, outsider, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("не состоите", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== GetUserChatsAsync ====================

    [Fact]
    public async Task GetUserChats_ReturnsChatsOrderedByLastMessage()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");
        var user3 = await CreateUserAsync("user3");

        var chat1 = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        Assert.True(chat1.IsSuccess);
        var chat1Value = chat1.Value;
        Assert.NotNull(chat1Value);

        _ = await _chatService.CreatePrivateChatAsync(user1, user3, CancellationToken.None);

        var messageService = ServiceProvider.GetRequiredService<IMessageService>();
        await messageService.SendMessageAsync(user1, new SendMessage { ChatId = chat1Value.Id, Text = "Latest" }, CancellationToken.None);

        var result = await _chatService.GetUserChatsAsync(user1, 0, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var chats = result.Value;
        Assert.NotNull(chats);
        var chatsList = chats.ToList(); // материализуем
        Assert.Equal(2, chatsList.Count);
        // Чат с последним сообщением должен быть первым
        Assert.Equal(chat1Value.Id, chatsList.First().ChatId);
    }

    // ==================== AddMemberAsync ====================

    [Fact]
    public async Task AddMember_AdminCanAddMember()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var newMember = await CreateUserAsync("newMember"); // исправлена опечатка
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            var role = await db.ChatMembers
                .Where(cm => cm.ChatId == chatValue.Id && cm.UserId == admin)
                .Select(cm => cm.Role)
                .FirstOrDefaultAsync();
            Assert.Equal(MemberRole.Admin, (MemberRole)role);
        }

        var result = await _chatService.AddMemberAsync(chatValue.Id, admin, newMember, CancellationToken.None);
        Assert.True(result.IsSuccess);

        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            var exists = await db.ChatMembers.AnyAsync(cm => cm.ChatId == chatValue.Id && cm.UserId == newMember);
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task AddMember_NotAdmin_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var newMember = await CreateUserAsync("newMember");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.AddMemberAsync(chatValue.Id, member, newMember, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("прав", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddMember_AlreadyInChat_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.AddMemberAsync(chatValue.Id, admin, member, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("уже в чате", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== RemoveMemberAsync ====================

    [Fact]
    public async Task RemoveMember_AdminRemovesMember()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.RemoveMemberAsync(chatValue.Id, admin, member, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chatValue.Id, CancellationToken.None);
        Assert.NotNull(updatedChat); // убираем !
        Assert.DoesNotContain(updatedChat.Members, m => m.UserId == member);
    }

    [Fact]
    public async Task RemoveMember_SelfRemoval()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.RemoveMemberAsync(chatValue.Id, member, member, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chatValue.Id, CancellationToken.None);
        Assert.NotNull(updatedChat);
        Assert.DoesNotContain(updatedChat.Members, m => m.UserId == member);
    }

    [Fact]
    public async Task RemoveMember_NotAdminAndNotSelf_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member1 = await CreateUserAsync("member1");
        var member2 = await CreateUserAsync("member2");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member1, member2], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.RemoveMemberAsync(chatValue.Id, member1, member2, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("прав", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== ChangeMemberRoleAsync ====================

    [Fact]
    public async Task ChangeMemberRole_AdminChangesRole()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.ChangeMemberRoleAsync(chatValue.Id, admin, member, MemberRole.Admin, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chatValue.Id, CancellationToken.None);
        Assert.NotNull(updatedChat);
        var updatedMember = updatedChat.Members.First(m => m.UserId == member);
        Assert.Equal(MemberRole.Admin, updatedMember.Role);
    }

    [Fact]
    public async Task ChangeMemberRole_NotAdmin_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member1 = await CreateUserAsync("member1");
        var member2 = await CreateUserAsync("member2");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member1, member2], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.ChangeMemberRoleAsync(chatValue.Id, member1, member2, MemberRole.Admin, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("прав", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== UpdateChatNameAsync ====================

    [Fact]
    public async Task UpdateChatName_AdminChangesName()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Old Name", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.UpdateChatNameAsync(chatValue.Id, admin, "New Name", CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chatValue.Id, CancellationToken.None);
        Assert.NotNull(updatedChat);
        Assert.Equal("New Name", updatedChat.Name);
    }

    [Fact]
    public async Task UpdateChatName_NotAdmin_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [member], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.UpdateChatNameAsync(chatValue.Id, member, "Hacked", CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("прав", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateChatName_EmptyName_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", [await CreateUserAsync("member")], CancellationToken.None);
        Assert.True(chat.IsSuccess);
        var chatValue = chat.Value;
        Assert.NotNull(chatValue);

        var result = await _chatService.UpdateChatNameAsync(chatValue.Id, admin, "", CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("не может быть пустым", error, StringComparison.OrdinalIgnoreCase);
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
}