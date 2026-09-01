using boltalka.Contracts.Models.Call;
using boltalka.Contracts.Models.Chat;
using boltalka.Contracts.Models.Media;
using boltalka.Contracts.Models.Message;

namespace boltalka.Contracts.Models.User;

/// <summary>
/// Пользователь.
/// </summary>
public class UserContract
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
    /// Логин (уникальное имя для входа).
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля (никогда не храним пароль в открытом виде).
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое имя (как видят другие пользователи).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Идентификатор файла аватара (null — нет аватара).</summary>
    public Guid? AvatarMediaId { get; set; }

    /// <summary>Навигационное свойство: файл аватара.</summary>
    public MediaContract? Avatar { get; set; }
    
    /// <summary>
    /// Возможность пользователя использовать функционал.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Связи с чатами, в которых состоит пользователь.
    /// </summary>
    public ICollection<ChatMemberContract> ChatMembers { get; set; } = new List<ChatMemberContract>();

    /// <summary>
    /// Сообщения, отправленные пользователем.
    /// </summary>
    public ICollection<MessageContract> Messages { get; set; } = new List<MessageContract>();

    /// <summary>
    /// Звонки, инициированные пользователем.
    /// </summary>
    public ICollection<CallContract> Calls { get; set; } = new List<CallContract>();
}