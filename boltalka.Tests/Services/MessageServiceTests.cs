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
using boltalka.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.Services;

public class MessageServiceTests : TestBase
{
    private readonly IMessageService _messageService;
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IUserRepository _userRepository;

    public MessageServiceTests()
    {
        _messageService = ServiceProvider.GetRequiredService<IMessageService>();
        _messageRepository = ServiceProvider.GetRequiredService<IMessageRepository>();
        _chatRepository = ServiceProvider.GetRequiredService<IChatRepository>();
        _mediaRepository = ServiceProvider.GetRequiredService<IMediaRepository>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
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
        var message = result.Value;
        Assert.NotNull(message);                     // устранение Dereference of possibly null reference
        Assert.Equal("Hello!", message.Text);
        Assert.Equal(chatId, message.ChatId);
        Assert.Equal(senderId, message.SenderId);
        Assert.Equal(MessageStatus.Sent, message.Status);
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
            MediaIds = [media1.Id, media2.Id]
        };
        var result = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = result.Value;
        Assert.NotNull(message);                     // устранение Dereference of possibly null reference
        Assert.NotNull(message.Attachments);
        var attachments = message.Attachments.ToList(); // материализуем, чтобы избежать multiple enumeration
        Assert.Equal(2, attachments.Count);
        Assert.Contains(attachments, a => a.Id == media1.Id);
        Assert.Contains(attachments, a => a.Id == media2.Id);
    }

    [Fact]
    public async Task SendMessage_EmptyTextAndNoMedia_ReturnsFailure()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "recipient");
        var dto = new SendMessage { ChatId = chatId };
        var result = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);                       // устранение возможного null для Error
        Assert.Contains("не может быть пустым", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendMessage_UserNotInChat_ReturnsFailure()
    {
        var (_, chatId, _) = await CreateChatWithTwoUsersAsync("alice", "bob");
        var outsiderId = await CreateUserAsync("outsider");

        var dto = new SendMessage { ChatId = chatId, Text = "Hi" };
        var result = await _messageService.SendMessageAsync(outsiderId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("не являетесь участником", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendMessage_NonExistingMedia_ReturnsFailure()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "recipient");
        var dto = new SendMessage
        {
            ChatId = chatId,
            Text = "Missing",
            MediaIds = [Guid.NewGuid()]
        };
        var result = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("Медиа с ID", error, StringComparison.OrdinalIgnoreCase);
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
        Assert.True(page2.IsSuccess);
        var list1 = page1.Value;
        var list2 = page2.Value;
        Assert.NotNull(list1);
        Assert.NotNull(list2);
        var messages1 = list1.ToList();    // материализуем
        var messages2 = list2.ToList();

        Assert.Equal(3, messages1.Count);  // теперь гарантированно не null
        Assert.Equal(3, messages2.Count);

        var ids1 = messages1.Select(m => m.Id).ToHashSet();
        var ids2 = messages2.Select(m => m.Id).ToHashSet();
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
        Assert.True(sendResult.IsSuccess);
        var sentMessage = sendResult.Value;
        Assert.NotNull(sentMessage);                 // устранение Dereference of possibly null reference

        var markResult = await _messageService.MarkAsReadAsync(sentMessage.Id, recipientId, CancellationToken.None);
        Assert.True(markResult.IsSuccess);

        var updatedMsg = await _messageRepository.GetByIdAsync(sentMessage.Id, CancellationToken.None);
        Assert.NotNull(updatedMsg);                  // вместо ! – явная проверка
        Assert.Equal(MessageStatus.Read, updatedMsg.Status);
    }

    [Fact]
    public async Task MarkAsRead_BySender_ReturnsFailure()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Hi" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);
        Assert.True(sendResult.IsSuccess);
        var sentMessage = sendResult.Value;
        Assert.NotNull(sentMessage);                 // устранение Dereference of possibly null reference

        var result = await _messageService.MarkAsReadAsync(sentMessage.Id, senderId, CancellationToken.None);
        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("своё сообщение", error, StringComparison.OrdinalIgnoreCase);
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
        Assert.True(sendResult.IsSuccess);
        var sentMessage = sendResult.Value;
        Assert.NotNull(sentMessage);                 // устранение Dereference

        var editResult = await _messageService.EditMessageAsync(sentMessage.Id, senderId, "New", CancellationToken.None);
        Assert.True(editResult.IsSuccess);
        var edited = editResult.Value;
        Assert.NotNull(edited);                      // устранение Dereference
        Assert.Equal("New", edited.Text);
        Assert.NotNull(edited.UpdatedAt);

        var msg = await _messageRepository.GetByIdAsync(sentMessage.Id, CancellationToken.None);
        Assert.NotNull(msg);                         // вместо !
        Assert.Equal("New", msg.Text);
    }

    [Fact]
    public async Task EditMessage_After24Hours_ReturnsFailure()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var sendDto = new SendMessage { ChatId = chatId, Text = "Old" };
        var sendResult = await _messageService.SendMessageAsync(senderId, sendDto, CancellationToken.None);
        Assert.True(sendResult.IsSuccess);
        var sentMessage = sendResult.Value;
        Assert.NotNull(sentMessage);                 // устранение Dereference

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    
        var message = await dbContext.Messages.FindAsync(sentMessage.Id);
        Assert.NotNull(message);                     // вместо !
        message.CreatedAt = DateTime.UtcNow.AddHours(-25);
        await dbContext.SaveChangesAsync();

        var scopedMessageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
        var editResult = await scopedMessageService.EditMessageAsync(sentMessage.Id, senderId, "New", CancellationToken.None);

        Assert.False(editResult.IsSuccess);
        var error = editResult.Error;
        Assert.NotNull(error);
        Assert.Contains("истекло", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EditMessage_NotSender_ReturnsFailure()
    {
        var (senderId, chatId, recipientId) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Secret" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);
        Assert.True(sendResult.IsSuccess);
        var sentMessage = sendResult.Value;
        Assert.NotNull(sentMessage);                 // устранение Dereference

        var result = await _messageService.EditMessageAsync(sentMessage.Id, recipientId, "Hacked", CancellationToken.None);
        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("свои сообщения", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== DeleteMessageAsync ====================

    [Fact]
    public async Task DeleteMessage_BySender_RemovesMessage()
    {
        var (senderId, chatId, _) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "ToDelete" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);
        Assert.True(sendResult.IsSuccess);
        var sentMessage = sendResult.Value;
        Assert.NotNull(sentMessage);                 // устранение Dereference

        var deleteResult = await _messageService.DeleteMessageAsync(sentMessage.Id, senderId, CancellationToken.None);
        Assert.True(deleteResult.IsSuccess);

        var msg = await _messageRepository.GetByIdAsync(sentMessage.Id, CancellationToken.None);
        Assert.Null(msg);
    }

    [Fact]
    public async Task DeleteMessage_NotSender_ReturnsFailure()
    {
        var (senderId, chatId, recipientId) = await CreateChatWithTwoUsersAsync("sender", "receiver");
        var dto = new SendMessage { ChatId = chatId, Text = "Keep" };
        var sendResult = await _messageService.SendMessageAsync(senderId, dto, CancellationToken.None);
        Assert.True(sendResult.IsSuccess);
        var sentMessage = sendResult.Value;
        Assert.NotNull(sentMessage);                 // устранение Dereference

        var result = await _messageService.DeleteMessageAsync(sentMessage.Id, recipientId, CancellationToken.None);
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
        await _userRepository.AddAsync(user, CancellationToken.None);
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
            Members =
            [
                new ChatMember { UserId = senderId, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow },
                new ChatMember { UserId = recipientId, ChatId = Guid.Empty, Role = MemberRole.Member, JoinedAt = DateTime.UtcNow }
            ]
        };
        foreach (var m in chat.Members) m.ChatId = chat.Id;
        await _chatRepository.AddAsync(chat, CancellationToken.None);
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
        await _mediaRepository.AddAsync(media, CancellationToken.None);
        return media;
    }
}