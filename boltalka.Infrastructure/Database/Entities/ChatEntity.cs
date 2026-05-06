using System.ComponentModel.DataAnnotations;
using boltalka.Infrastructure.Database.Abstract;
using boltalka.Infrastructure.Database.Enum;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Чат (личный или групповой).
/// </summary>
public class ChatEntity : BaseEntity
{
    /// <summary>
    /// Название чата (для групповых; у личных может быть null или пустым).
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Тип чата: Private (1 на 1) или Group.
    /// </summary>
    [Required]
    public ChatType Type { get; set; }

    /// <summary>
    /// Участники чата.
    /// </summary>
    public ICollection<ChatMemberEntity> Members { get; set; } = new List<ChatMemberEntity>();

    /// <summary>
    /// Сообщения в чате.
    /// </summary>
    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();

    /// <summary>
    /// Звонки, совершённые в этом чате.
    /// </summary>
    public ICollection<CallEntity> Calls { get; set; } = new List<CallEntity>();
}