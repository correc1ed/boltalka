using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Chat;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models.Message;
using boltalka.Application.Models.User;
using boltalka.Application.Tests.Infrastructure;
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
        Assert.Equal(ChatType.Private, result.Value.Type);
        Assert.Null(result.Value.Name);

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var chatId = result.Value.Id;
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
        Assert.Contains("самим собой", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreatePrivateChat_AlreadyExists_ReturnsExisting()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");

        // Создаём первый чат
        var firstResult = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);

        // Повторный запрос с теми же участниками
        var secondResult = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(firstResult.Value.Id, secondResult.Value.Id); // тот же чат
    }

    [Fact]
    public async Task CreatePrivateChat_NonExistingUser_ReturnsFailure()
    {
        var user1 = await CreateUserAsync("user1");
        var result = await _chatService.CreatePrivateChatAsync(user1, Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не найден", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== CreateGroupChatAsync ====================

    [Fact]
    public async Task CreateGroupChat_ValidData_CreatesChat()
    {
        var admin = await CreateUserAsync("admin");
        var member1 = await CreateUserAsync("member1");
        var member2 = await CreateUserAsync("member2");

        var result = await _chatService.CreateGroupChatAsync(admin, "Test Group", new[] { member1, member2 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test Group", result.Value.Name);
        Assert.Equal(ChatType.Group, result.Value.Type);

        // Проверяем состав участников и их роли через прямую работу с БД
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var chatId = result.Value.Id;

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
        Assert.Contains("не может быть пустым", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateGroupChat_NotEnoughMembers_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        // Только создатель, без дополнительных участников
        var result = await _chatService.CreateGroupChatAsync(admin, "Solo", new List<Guid>(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Недостаточно", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateGroupChat_NonExistingUser_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var memberIds = new List<Guid> { Guid.NewGuid() };
        var result = await _chatService.CreateGroupChatAsync(admin, "Invalid", memberIds, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не найден", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== GetChatAsync ====================

    [Fact]
    public async Task GetChat_Member_ReturnsChatWithMembers()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");
        var createResult = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);

        var result = await _chatService.GetChatAsync(createResult.Value.Id, user1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value.Id, result.Value.Id);
        Assert.Equal(2, result.Value.Members?.Count);
    }

    [Fact]
    public async Task GetChat_NotMember_ReturnsFailure()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");
        var outsider = await CreateUserAsync("outsider");
        var chat = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);

        var result = await _chatService.GetChatAsync(chat.Value.Id, outsider, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не состоите", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== GetUserChatsAsync ====================

    [Fact]
    public async Task GetUserChats_ReturnsChatsOrderedByLastMessage()
    {
        var user1 = await CreateUserAsync("user1");
        var user2 = await CreateUserAsync("user2");
        var user3 = await CreateUserAsync("user3");

        var chat1 = await _chatService.CreatePrivateChatAsync(user1, user2, CancellationToken.None);
        var chat2 = await _chatService.CreatePrivateChatAsync(user1, user3, CancellationToken.None);

        // Чтобы проверить сортировку, можно создать сообщение в одном из чатов
        var messageService = ServiceProvider.GetRequiredService<IMessageService>();
        await messageService.SendMessageAsync(user1, new SendMessage { ChatId = chat1.Value.Id, Text = "Latest" }, CancellationToken.None);

        var result = await _chatService.GetUserChatsAsync(user1, 0, 10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        // Чат с последним сообщением должен быть первым
        Assert.Equal(chat1.Value.Id, result.Value.First().ChatId);
    }

    // ==================== AddMemberAsync ====================

    [Fact]
    public async Task AddMember_AdminCanAddMember()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var newMember = await CreateUserAsync("newmember");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member }, CancellationToken.None);

        // Явная проверка роли админа в БД (опционально, но оставлю для уверенности)
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            var role = await db.ChatMembers
                .Where(cm => cm.ChatId == chat.Value.Id && cm.UserId == admin)
                .Select(cm => cm.Role)
                .FirstOrDefaultAsync();
            Assert.Equal(MemberRole.Admin, (MemberRole)role);
        }

        var result = await _chatService.AddMemberAsync(chat.Value.Id, admin, newMember, CancellationToken.None);
        Assert.True(result.IsSuccess);

        // Проверяем, что запись появилась
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            var exists = await db.ChatMembers.AnyAsync(cm => cm.ChatId == chat.Value.Id && cm.UserId == newMember);
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task AddMember_NotAdmin_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var newMember = await CreateUserAsync("newmember");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member }, CancellationToken.None);

        var result = await _chatService.AddMemberAsync(chat.Value.Id, member, newMember, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("прав", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddMember_AlreadyInChat_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member }, CancellationToken.None);

        var result = await _chatService.AddMemberAsync(chat.Value.Id, admin, member, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("уже в чате", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== RemoveMemberAsync ====================

    [Fact]
    public async Task RemoveMember_AdminRemovesMember()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member }, CancellationToken.None);

        var result = await _chatService.RemoveMemberAsync(chat.Value.Id, admin, member, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chat.Value.Id, CancellationToken.None);
        Assert.DoesNotContain(updatedChat!.Members, m => m.UserId == member);
    }

    [Fact]
    public async Task RemoveMember_SelfRemoval()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member }, CancellationToken.None);

        var result = await _chatService.RemoveMemberAsync(chat.Value.Id, member, member, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chat.Value.Id, CancellationToken.None);
        Assert.DoesNotContain(updatedChat!.Members, m => m.UserId == member);
    }

    [Fact]
    public async Task RemoveMember_NotAdminAndNotSelf_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member1 = await CreateUserAsync("member1");
        var member2 = await CreateUserAsync("member2");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member1, member2 }, CancellationToken.None);

        // member1 пытается удалить member2
        var result = await _chatService.RemoveMemberAsync(chat.Value.Id, member1, member2, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("прав", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== ChangeMemberRoleAsync ====================

    [Fact]
    public async Task ChangeMemberRole_AdminChangesRole()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member }, CancellationToken.None);

        var result = await _chatService.ChangeMemberRoleAsync(chat.Value.Id, admin, member, MemberRole.Admin, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chat.Value.Id, CancellationToken.None);
        var updatedMember = updatedChat!.Members.First(m => m.UserId == member);
        Assert.Equal(MemberRole.Admin, updatedMember.Role);
    }

    [Fact]
    public async Task ChangeMemberRole_NotAdmin_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member1 = await CreateUserAsync("member1");
        var member2 = await CreateUserAsync("member2");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member1, member2 }, CancellationToken.None);

        var result = await _chatService.ChangeMemberRoleAsync(chat.Value.Id, member1, member2, MemberRole.Admin, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("прав", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== UpdateChatNameAsync ====================

    [Fact]
    public async Task UpdateChatName_AdminChangesName()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Old Name", new[] { member }, CancellationToken.None);

        var result = await _chatService.UpdateChatNameAsync(chat.Value.Id, admin, "New Name", CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedChat = await _chatRepository.GetChatWithMembersAsync(chat.Value.Id, CancellationToken.None);
        Assert.Equal("New Name", updatedChat!.Name);
    }

    [Fact]
    public async Task UpdateChatName_NotAdmin_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var member = await CreateUserAsync("member");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { member }, CancellationToken.None);

        var result = await _chatService.UpdateChatNameAsync(chat.Value.Id, member, "Hacked", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("прав", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateChatName_EmptyName_ReturnsFailure()
    {
        var admin = await CreateUserAsync("admin");
        var chat = await _chatService.CreateGroupChatAsync(admin, "Group", new[] { await CreateUserAsync("member") }, CancellationToken.None);

        var result = await _chatService.UpdateChatNameAsync(chat.Value.Id, admin, "", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не может быть пустым", result.Error, StringComparison.OrdinalIgnoreCase);
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