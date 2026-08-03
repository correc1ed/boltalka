using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Message;
using boltalka.Application.Models;
using boltalka.Application.Models.Message;
using boltalka.Application.Models.MessageMedia;

namespace boltalka.Application.UseCases.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public MessageService(
        IMessageRepository messageRepository,
        IMediaRepository mediaRepository,
        IChatRepository chatRepository,
        IMapper mapper,
        INotificationService notificationService)
    {
        _messageRepository = messageRepository;
        _mediaRepository = mediaRepository;
        _chatRepository = chatRepository;
        _mapper = mapper;
        _notificationService = notificationService;
    }
    
    public async Task<Result<MessageInfo>> SendMessageAsync(Guid senderId, SendMessage sendMessage, CancellationToken cancellationToken)
    {
        if (!await _chatRepository.IsUserInChatAsync(sendMessage.ChatId, senderId, cancellationToken))
            return Result<MessageInfo>.Failure("Вы не являетесь участником чата.");

        if (string.IsNullOrWhiteSpace(sendMessage.Text) && (sendMessage.MediaIds == null || sendMessage.MediaIds.Count == 0))
            return Result<MessageInfo>.Failure("Сообщение не может быть пустым.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = sendMessage.ChatId,
            SenderId = senderId,
            Text = sendMessage.Text,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };

        if (sendMessage.MediaIds?.Count > 0)
        {
            var mediaLinks = new List<MessageMedia>();
            var sortOrder = 0;

            foreach (var mediaId in sendMessage.MediaIds)
            {
                var media = await _mediaRepository.GetByIdAsync(mediaId, cancellationToken);
                if (media == null)
                    return Result<MessageInfo>.Failure($"Медиа с ID {mediaId} не найдено.");

                mediaLinks.Add(new MessageMedia
                {
                    MessageId = message.Id,
                    MediaId = mediaId,
                    SortOrder = sortOrder++
                });
            }

            message.MessageMediaLinks = mediaLinks;
        }

        await _messageRepository.AddAsync(message, cancellationToken);

        var savedMessage = await _messageRepository.GetMessageWithMediaAsync(message.Id, cancellationToken);
        
        var messageDto = _mapper.Map<MessageInfo>(savedMessage);
        
        var messageForNotification = _mapper.Map<Message>(savedMessage);

        await _notificationService.NotifyNewMessageAsync(sendMessage.ChatId, messageForNotification, cancellationToken, excludeUserIds: new[] { senderId });

        return Result<MessageInfo>.Success(messageDto);
    }

    public async Task<Result<IEnumerable<Message>>> GetMessagesAsync(Guid chatId, Guid userId, int skip, int take, CancellationToken cancellationToken,
        DateTime? before = null)
    {
        if (!await _chatRepository.IsUserInChatAsync(chatId, userId, cancellationToken))
            return Result<IEnumerable<Message>>.Failure("Вы не состоите в этом чате.");

        var messages = await _messageRepository.GetMessagesAsync(chatId, skip, take, cancellationToken, before);
        var messageDtos = _mapper.Map<IEnumerable<Message>>(messages);

        return Result<IEnumerable<Message>>.Success(messageDtos);
    }

    public async Task<Result> MarkAsReadAsync(Guid messageId, Guid userId, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);
        if (message is null)
            return Result.Failure("Сообщение не найдено.");

        if (!await _chatRepository.IsUserInChatAsync(message.ChatId, userId, cancellationToken))
            return Result.Failure("Вы не состоите в чате этого сообщения.");

        if (message.Status == MessageStatus.Read)
            return Result.Success();

        if (message.SenderId == userId)
            return Result.Failure("Нельзя отметить своё сообщение как прочитанное.");

        await _messageRepository.UpdateStatusAsync(messageId, MessageStatus.Read, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<MessageInfo>> EditMessageAsync(Guid messageId, Guid userId, string newText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newText))
            return Result<MessageInfo>.Failure("Текст сообщения не может быть пустым.");

        var message = await _messageRepository.GetMessageWithMediaAsync(messageId, cancellationToken);
        if (message is null)
            return Result<MessageInfo>.Failure("Сообщение не найдено.");

        if (message.SenderId != userId)
            return Result<MessageInfo>.Failure("Вы можете редактировать только свои сообщения.");

        if (DateTime.UtcNow - message.CreatedAt > TimeSpan.FromHours(24))
            return Result<MessageInfo>.Failure("Время редактирования истекло.");

        message.Text = newText;
        message.UpdatedAt = DateTime.UtcNow;
        await _messageRepository.UpdateAsync(message, cancellationToken);

        var updatedDto = _mapper.Map<MessageInfo>(message);
        
        await _notificationService.NotifySystemMessageAsync(message.ChatId, $"Сообщение отредактировано.", cancellationToken);

        return Result<MessageInfo>.Success(updatedDto);
    }

    public async Task<Result> DeleteMessageAsync(Guid messageId, Guid userId, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);
        if (message is null)
            return Result.Failure("Сообщение не найдено.");

        if (message.SenderId != userId)
            return Result.Failure("Вы можете удалять только свои сообщения.");

        await _messageRepository.DeleteAsync(message, cancellationToken);

        await _notificationService.NotifySystemMessageAsync(message.ChatId, "Сообщение удалено.", cancellationToken);

        return Result.Success();
    }
}