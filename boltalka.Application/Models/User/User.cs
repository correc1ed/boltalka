namespace boltalka.Application.Models.User;

/// <summary>
/// Пользователь.
/// </summary>
public class User
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
    public Media.Media? Avatar { get; set; }
    
    /// <summary>
    /// Возможность пользователя использовать функционал.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Связи с чатами, в которых состоит пользователь.
    /// </summary>
    public ICollection<ChatMember.ChatMember> ChatMembers { get; set; } = new List<ChatMember.ChatMember>();

    /// <summary>
    /// Сообщения, отправленные пользователем.
    /// </summary>
    public ICollection<Message.Message> Messages { get; set; } = new List<Message.Message>();

    /// <summary>
    /// Звонки, инициированные пользователем.
    /// </summary>
    public ICollection<Call.Call> Calls { get; set; } = new List<Call.Call>();
}