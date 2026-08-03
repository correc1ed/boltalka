using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Chat;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Enums.Message;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;
using boltalka.Application.Models.Media;
using boltalka.Application.Models.Message;
using boltalka.Application.Models.User;
using boltalka.Application.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.Services;

public class MessageServiceTests : TestBase
{
    private readonly IMessageService _messageService;
    private readonly IMessageRepository _messageRepo;
    private readonly IChatRepository _chatRepo;
    private readonly IMediaRepository _mediaRepo;
    private readonly IUserRepository _userRepo;

    public MessageServiceTests()
    {
        _messageService = ServiceProvider.GetRequiredService<IMessageService>();
        _messageRepo = ServiceProvider.GetRequiredService<IMessageRepository>();
        _chatRepo = ServiceProvider.GetRequiredService<IChatRepository>();
        _mediaRepo = ServiceProvider.GetRequiredService<IMediaRepository>();
        _userRepo = ServiceProvider.GetRequiredService<IUserRepository>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMessageService, MessageService>();
    }

    // ==================== SendMessageAsync ====================

    [Fact]
    public async Task SendMessage_TextOnly_ReturnsMessageDto()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "recipient");

        var dto = new SendMessage { ChatId = chatId, Text = "Hello!" };
        var result = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Hello!", result.Value.Text);
        Assert.Equal(chatId, result.Value.ChatId);
        Assert.Equal(senderId, result.Value.SenderId);
        Assert.Equal(MessageStatus.Sent, result.Value.Status);
    }

    [Fact]
    public async Task SendMessage_WithAttachments_ReturnsMessageDto()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "recipient");
        var media1 = await CreateMediaAsync("file1.png", "image/png");
        var media2 = await CreateMediaAsync("file2.pdf", "application/pdf");

        var dto = new SendMessage
        {
            ChatId = chatId,
            Text = "Files",
            MediaIds = new List<Guid> { media1.Id, media2.Id }
        };
        var result = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Attachments.Count());
        Assert.Contains(result.Value.Attachments, a => a.Id == media1.Id);
        Assert.Contains(result.Value.Attachments, a => a.Id == media2.Id);
    }

    [Fact]
    public async Task SendMessage_EmptyTextAndNoMedia_ReturnsFailure()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "recipient");
        var dto = new SendMessage { ChatId = chatId };
        var result = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не может быть пустым", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendMessage_UserNotInChat_ReturnsFailure()
    {
        var (_, chatId, _) = await CreateChatWithTwoUsersAsync("alice", "bob");
        var outsiderId = await CreateUserAsync("outsider");

        var dto = new SendMessage { ChatId = chatId, Text = "Hi" };
        var result = await _messageService.SendMessageAsync(outsiderId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не являетесь участником", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendMessage_NonExistingMedia_ReturnsFailure()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "recipient");
        var dto = new SendMessage
        {
            ChatId = chatId,
            Text = "Missing",
            MediaIds = new List<Guid> { Guid.NewGuid() }
        };
        var result = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Медиа с ID", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== GetMessagesAsync ====================

    [Fact]
    public async Task GetMessages_Pagination_ReturnsCorrectPage()
    {
        var (senderId, chatId, recipientId) = await CreateChatWithTwoUsersAsync("alice", "bob");
        for (int i = 0; i < 10; i++)
        {
            var dto = new SendMessage { ChatId = chatId, Text = $"msg{i}" };
            await _messageService.SendMessageAsync(i % 2 == 0 ? senderId : recipientId, dto, CancellationToken.None);
        }

        var page1 = await _messageService.GetMessagesAsync(chatId, senderId, skip: 0, take: 3, CancellationToken.None);
        var page2 = await _messageService.GetMessagesAsync(chatId, senderId, skip: 3, take: 3, CancellationToken.None);

        Assert.True(page1.IsSuccess);
        Assert.Equal(3, page1.Value.Count());
        Assert.True(page2.IsSuccess);
        Assert.Equal(3, page2.Value.Count());

        var ids1 = page1.Value.Select(m => m.Id).ToHashSet();
        var ids2 = page2.Value.Select(m => m.Id).ToHashSet();
        Assert.False(ids1.Overlaps(ids2));
    }

    [Fact]
    public async Task GetMessages_NotAMember_ReturnsFailure()
    {
        var (_, chatId, _) = await CreateChatWithTwoUsersAsync("alice", "bob");
        var outsiderId = await CreateUserAsync("outsider");

        var result = await _messageService.GetMessagesAsync(chatId, outsiderId, 0, 10, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== MarkAsReadAsync ====================

    [Fact]
    public async Task MarkAsRead_ByRecipient_SetsReadStatus()
    {
        var (senderId, chatId, recipientId) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Hi" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        var markResult = await _messageService.MarkAsReadAsync(sendResult.Value.Id, recipientId, CancellationToken.None);
        Assert.True(markResult.IsSuccess);

        var updatedMsg = await _messageRepo.GetByIdAsync(sendResult.Value.Id, CancellationToken.None);
        Assert.Equal(MessageStatus.Read, updatedMsg!.Status);
    }

    [Fact]
    public async Task MarkAsRead_BySender_ReturnsFailure()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Hi" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        var result = await _messageService.MarkAsReadAsync(sendResult.Value.Id, senderId, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Contains("своё сообщение", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MarkAsRead_NonExistingMessage_ReturnsFailure()
    {
        var userId = await CreateUserAsync("user");
        var result = await _messageService.MarkAsReadAsync(Guid.NewGuid(), userId, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== EditMessageAsync ====================

    [Fact]
    public async Task EditMessage_BySenderWithin24Hours_UpdatesText()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Old" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        var editResult = await _messageService.EditMessageAsync(sendResult.Value.Id, senderId, "New", CancellationToken.None);

        Assert.True(editResult.IsSuccess);
        Assert.Equal("New", editResult.Value.Text);
        Assert.NotNull(editResult.Value.UpdatedAt);

        var msg = await _messageRepo.GetByIdAsync(sendResult.Value.Id, CancellationToken.None);
        Assert.Equal("New", msg!.Text);
    }

    [Fact]
    public async Task EditMessage_After24Hours_ReturnsFailure()
    {
        // Arrange
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var sendDto = new SendMessage { ChatId = chatId, Text = "Old" };
        var sendResult = await _messageService.SendMessageAsync(senderId, sendDto, CancellationToken.None);

        // Получаем тот же контекст, что и у сервисов
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    
        // Меняем CreatedAt на 25 часов назад
        var message = await dbContext.Messages.FindAsync(sendResult.Value.Id);
        message!.CreatedAt = DateTime.UtcNow.AddHours(-25);
        await dbContext.SaveChangesAsync();

        // Act: вызываем редактирование в том же scope (через сервис из scope)
        var scopedMessageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
        var editResult = await scopedMessageService.EditMessageAsync(sendResult.Value.Id, senderId, "New", CancellationToken.None);

        // Assert
        Assert.False(editResult.IsSuccess);
        Assert.Contains("истекло", editResult.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EditMessage_NotSender_ReturnsFailure()
    {
        var (senderId, chatId, recipientId) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Secret" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        var result = await _messageService.EditMessageAsync(sendResult.Value.Id, recipientId, "Hacked", CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Contains("свои сообщения", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== DeleteMessageAsync ====================

    [Fact]
    public async Task DeleteMessage_BySender_RemovesMessage()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "ToDelete" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        var deleteResult = await _messageService.DeleteMessageAsync(sendResult.Value.Id, senderId, CancellationToken.None);
        Assert.True(deleteResult.IsSuccess);

        var msg = await _messageRepo.GetByIdAsync(sendResult.Value.Id, CancellationToken.None);
        Assert.Null(msg);
    }

    [Fact]
    public async Task DeleteMessage_NotSender_ReturnsFailure()
    {
        var (senderId, chatId, recipientId) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Keep" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        var result = await _messageService.DeleteMessageAsync(sendResult.Value.Id, recipientId, CancellationToken.None);
        Assert.False(result.IsSuccess);
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
        await _userRepo.AddAsync(user, CancellationToken.None);
        return user.Id;
    }

    private async Task<(Guid senderId, Guid chatId, Guid recipientId)> CreateChatWithTwoUsersAsync(string senderLogin, string recipientLogin)
    {
        var senderId = await CreateUserAsync(senderLogin);
        var recipientId = await CreateUserAsync(recipientLogin);

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = null,
            Type = ChatType.Private,
            CreatedAt = DateTime.UtcNow,
            Members = new List<ChatMember>
            {
                new ChatMember { UserId = senderId, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow },
                new ChatMember { UserId = recipientId, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow }
            }
        };
        foreach (var m in chat.Members) m.ChatId = chat.Id;
        await _chatRepo.AddAsync(chat, CancellationToken.None);
        return (senderId, chat.Id, recipientId);
    }

    private async Task<Media> CreateMediaAsync(string fileName, string contentType)
    {
        var media = new Media
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = 1234,
            StoragePath = $"fake/path/{Guid.NewGuid()}",
            UploadedByUserId = null,
            CreatedAt = DateTime.UtcNow
        };
        await _mediaRepo.AddAsync(media, CancellationToken.None);
        return media;
    }
}