using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Call;
using boltalka.Application.Models.Message;
using boltalka.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace boltalka.WebApi.UseCases.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public NotificationService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewMessageAsync(Guid chatId, Message message, CancellationToken cancellationToken, IEnumerable<Guid>? excludeUserIds = null)
    {
        // В реальном приложении здесь нужно получить список ConnectionId участников чата
        // и исключить отправителя. Пока вызываем метод у всей группы.
        await _hubContext.Clients.Group(chatId.ToString()).SendAsync("NewMessage", message);
    }

    public async Task NotifyIncomingCallAsync(Guid chatId, Call call, CancellationToken cancellationToken, IEnumerable<Guid>? excludeUserIds = null)
    {
        var group = _hubContext.Clients.Group(chatId.ToString());
        if (excludeUserIds?.Any() == true)
        {
            // Здесь логика исключения конкретных ConnectionId (или просто отправляем всем, т.к. SignalR не умеет исключать по userId "из коробки" без маппинга)
            // Для MVP можно отправить всем, а клиент сам проигнорирует, если он инициатор.
            // Но лучше реализовать исключение через кастомный UserId -> ConnectionId словарь.
            await group.SendAsync("IncomingCall", call, cancellationToken);
        }
        else
        {
            await group.SendAsync("IncomingCall", call, cancellationToken);
        }
    }

    public async Task NotifyCallStatusChangedAsync(Guid chatId, Call call, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(chatId.ToString()).SendAsync("CallStatusChanged", call);
    }

    public async Task NotifyMemberAddedAsync(Guid chatId, Guid newUserId, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(chatId.ToString()).SendAsync("MemberAdded", new { ChatId = chatId, UserId = newUserId });
    }

    public async Task NotifyMemberRemovedAsync(Guid chatId, Guid removedUserId, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(chatId.ToString()).SendAsync("MemberRemoved", new { ChatId = chatId, UserId = removedUserId });
    }

    public async Task NotifySystemMessageAsync(Guid chatId, string text, CancellationToken cancellationToken)
    {
        var systemMessage = new { ChatId = chatId, Text = text, Timestamp = DateTime.UtcNow };
        await _hubContext.Clients.Group(chatId.ToString()).SendAsync("SystemMessage", systemMessage);
    }
}