using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Chat;
using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;
using boltalka.Application.Models.Message;
using boltalka.Application.Models.MessageMedia;
using boltalka.Application.Models.User;
using boltalka.Tests.Infrastructure;
using boltalka.Contracts.Models.Message;
using boltalka.Infrastructure.Database;
using boltalka.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.WebApi;

public class MessagesControllerTests : TestBase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;
    private readonly MessagesController _controller;
    private readonly ServiceDbContext _dbContext;

    public MessagesControllerTests()
    {
        var messageService = ServiceProvider.GetRequiredService<IMessageService>();
        _messageRepository = ServiceProvider.GetRequiredService<IMessageRepository>();
        _chatRepository = ServiceProvider.GetRequiredService<IChatRepository>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        _controller = new MessagesController(messageService, mapper);
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
        return (user1Id, user2Id, chat.Id);
    }

    private async Task<Message> CreateMessageAsync(Guid senderId, Guid chatId, string text, List<Guid>? mediaIds = null)
    {
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = senderId,
            Text = text,
            Status = (Application.Enums.Message.MessageStatus)MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow,
            MessageMediaLinks = mediaIds?.Select((mediaId, index) => new MessageMedia
            {
                MessageId = Guid.NewGuid(), // будет заменено
                MediaId = mediaId,
                SortOrder = index
            }).ToList() ?? new List<MessageMedia>()
        };
        // Fix MessageId в ссылках
        foreach (var link in message.MessageMediaLinks)
            link.MessageId = message.Id;

        await _messageRepository.AddAsync(message, CancellationToken.None);
        return message;
    }

    // ==================== SendMessage ====================

    [Fact]
    public async Task SendMessage_ValidText_ReturnsOkWithMessage()
    {
        // Arrange
        var (senderId, _, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        SetupAuthenticatedUser(senderId);
        var contract = new SendMessageContract { ChatId = chatId, Text = "Hello!" };

        // Act
        var result = await _controller.SendMessageAsync(chatId, contract, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var messageInfo = Assert.IsType<MessageInfoContract>(okResult.Value);
        Assert.Equal("Hello!", messageInfo.Text);
        Assert.Equal(chatId, messageInfo.ChatId);
        Assert.Equal(senderId, messageInfo.SenderId);
    }

    [Fact]
    public async Task SendMessage_EmptyTextAndNoMedia_ReturnsBadRequest()
    {
        // Arrange
        var (senderId, _, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        SetupAuthenticatedUser(senderId);
        var contract = new SendMessageContract { ChatId = chatId };

        // Act
        var result = await _controller.SendMessageAsync(chatId, contract, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendMessage_UserNotInChat_ReturnsBadRequest()
    {
        // Arrange
        var (_, _, chatId) = await CreatePrivateChatAsync("alice", "bob");
        var outsiderId = await CreateUserAsync("outsider");
        SetupAuthenticatedUser(outsiderId);
        var contract = new SendMessageContract { ChatId = chatId, Text = "Hi" };

        // Act
        var result = await _controller.SendMessageAsync(chatId, contract, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendMessage_NonExistingMedia_ReturnsBadRequest()
    {
        // Arrange
        var (senderId, _, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        SetupAuthenticatedUser(senderId);
        var contract = new SendMessageContract
        {
            ChatId = chatId,
            Text = "with file",
            MediaIds = [Guid.NewGuid()]
        };

        // Act
        var result = await _controller.SendMessageAsync(chatId, contract, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== GetMessages ====================

    [Fact]
    public async Task GetMessages_UserIsMember_ReturnsOkWithList()
    {
        // Arrange
        var (senderId, receiverId, chatId) = await CreatePrivateChatAsync("alice", "bob");
        _ = await CreateMessageAsync(senderId, chatId, "Hello");
        _ = await CreateMessageAsync(receiverId, chatId, "Hi there");
        SetupAuthenticatedUser(senderId);

        // Act
        var result = await _controller.GetMessagesAsync(chatId, 0, 50, null, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var messages = Assert.IsAssignableFrom<IEnumerable<MessageContract>>(okResult.Value);
        Assert.Equal(2, messages.Count());
    }

    [Fact]
    public async Task GetMessages_UserNotInChat_ReturnsBadRequest()
    {
        // Arrange
        var (_, _, chatId) = await CreatePrivateChatAsync("alice", "bob");
        var outsiderId = await CreateUserAsync("outsider");
        SetupAuthenticatedUser(outsiderId);

        // Act
        var result = await _controller.GetMessagesAsync(chatId, 0, 50, null, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== MarkAsRead ====================

    [Fact]
    public async Task MarkAsRead_ByReceiver_ReturnsOk()
    {
        // Arrange
        var (senderId, receiverId, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        var message = await CreateMessageAsync(senderId, chatId, "Read me");
        SetupAuthenticatedUser(receiverId);

        // Act
        var result = await _controller.MarkAsReadAsync(message.Id, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);

        // Проверяем статус в БД
        var updated = await _dbContext.Messages.FindAsync(message.Id);
        Assert.Equal(nameof(MessageStatus.Read), updated!.Status.ToString());
    }

    [Fact]
    public async Task MarkAsRead_BySender_ReturnsBadRequest()
    {
        // Arrange
        var (senderId, _, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        var message = await CreateMessageAsync(senderId, chatId, "My own");
        SetupAuthenticatedUser(senderId);

        // Act
        var result = await _controller.MarkAsReadAsync(message.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task MarkAsRead_NonExistingMessage_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("user");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.MarkAsReadAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== EditMessage ====================

    [Fact]
    public async Task EditMessage_SenderWithin24Hours_ReturnsOkWithUpdated()
    {
        // Arrange
        var (senderId, _, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        var message = await CreateMessageAsync(senderId, chatId, "Old text");
        SetupAuthenticatedUser(senderId);

        // Act
        var result = await _controller.EditMessageAsync(message.Id, "New text", CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedInfo = Assert.IsType<MessageInfoContract>(okResult.Value);
        Assert.Equal("New text", updatedInfo.Text);

        var dbMessage = await _dbContext.Messages.FindAsync(message.Id);
        Assert.Equal("New text", dbMessage!.Text);
    }

    [Fact]
    public async Task EditMessage_NotSender_ReturnsBadRequest()
    {
        // Arrange
        var (senderId, receiverId, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        var message = await CreateMessageAsync(senderId, chatId, "Original");
        SetupAuthenticatedUser(receiverId);

        // Act
        var result = await _controller.EditMessageAsync(message.Id, "Hacked", CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EditMessage_After24Hours_ReturnsBadRequest()
    {
        // Arrange
        var (senderId, _, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        var message = await CreateMessageAsync(senderId, chatId, "Old");
        SetupAuthenticatedUser(senderId);

        // Сдвигаем CreatedAt на 25 часов назад
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
            var entity = await db.Messages.FindAsync(message.Id);
            entity!.CreatedAt = DateTime.UtcNow.AddHours(-25);
            await db.SaveChangesAsync();
        }

        // Act
        var result = await _controller.EditMessageAsync(message.Id, "New", CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== DeleteMessage ====================

    [Fact]
    public async Task DeleteMessage_Sender_ReturnsOk()
    {
        // Arrange
        var (senderId, _, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        var message = await CreateMessageAsync(senderId, chatId, "Delete me");
        SetupAuthenticatedUser(senderId);

        // Act
        var result = await _controller.DeleteMessageAsync(message.Id, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);

        var exists = await _dbContext.Messages.AnyAsync(m => m.Id == message.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteMessage_NotSender_ReturnsBadRequest()
    {
        // Arrange
        var (senderId, receiverId, chatId) = await CreatePrivateChatAsync("sender", "receiver");
        var message = await CreateMessageAsync(senderId, chatId, "Keep");
        SetupAuthenticatedUser(receiverId);

        // Act
        var result = await _controller.DeleteMessageAsync(message.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}