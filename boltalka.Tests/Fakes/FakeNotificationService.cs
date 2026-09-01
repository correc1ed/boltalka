using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Call;
using boltalka.Application.Models.Message;

namespace boltalka.Tests.Fakes;

public class FakeNotificationService : INotificationService
{
    public Task NotifyNewMessageAsync(Guid chatId, Message message, CancellationToken cancellationToken,
        IEnumerable<Guid>? excludeUserIds = null)
        => Task.CompletedTask;

    public Task NotifyIncomingCallAsync(Guid chatId, Call call, CancellationToken cancellationToken,
        IEnumerable<Guid>? excludeUserIds = null)
        => Task.CompletedTask;

    public Task NotifyCallStatusChangedAsync(Guid chatId, Call call, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifyMemberAddedAsync(Guid chatId, Guid newUserId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifyMemberRemovedAsync(Guid chatId, Guid removedUserId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifySystemMessageAsync(Guid chatId, string text, CancellationToken ct = default)
        => Task.CompletedTask;
}