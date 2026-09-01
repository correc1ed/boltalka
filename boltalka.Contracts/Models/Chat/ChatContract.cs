using boltalka.Contracts.Models.Call;
using boltalka.Contracts.Models.Message;

namespace boltalka.Contracts.Models.Chat;

public class ChatContract
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Название чата (для групповых; у личных может быть null или пустым).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Тип чата: Private (1 на 1) или Group.
    /// </summary>
    public ChatType Type { get; set; }
    /// <summary>
    /// Участники чата.
    /// </summary>
    public ICollection<ChatMemberContract> Members { get; set; } = new List<ChatMemberContract>();

    /// <summary>
    /// Сообщения в чате.
    /// </summary>
    public ICollection<MessageContract> Messages { get; set; } = new List<MessageContract>();

    /// <summary>
    /// Звонки, совершённые в этом чате.
    /// </summary>
    public ICollection<CallContract> Calls { get; set; } = new List<CallContract>();
}