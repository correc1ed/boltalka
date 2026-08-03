using Microsoft.AspNetCore.SignalR;

namespace boltalka.WebApi.Hubs;

public class ChatHub : Hub
{
    // Метод для присоединения к группе чата
    public async Task JoinChatGroup(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    // Метод для выхода из группы чата
    public async Task LeaveChatGroup(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }
}