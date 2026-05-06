using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using boltalka.Infrastructure.Database.Abstract;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Пользователь мессенджера.
/// </summary>
[Index(nameof(Login), IsUnique = true)]
public class UserEntity : BaseEntity
{
    /// <summary>
    /// Логин (уникальное имя для входа).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля (никогда не храним пароль в открытом виде).
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое имя (как видят другие пользователи).
    /// </summary>
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Идентификатор файла аватара (null — нет аватара).</summary>
    public Guid? AvatarMediaId { get; set; }

    /// <summary>Навигационное свойство: файл аватара.</summary>
    public MediaEntity? Avatar { get; set; }
    
    /// <summary>
    /// Возможность пользователя использовать функционал.
    /// </summary>
    [Required]
    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Связи с чатами, в которых состоит пользователь.
    /// </summary>
    public ICollection<ChatMemberEntity> ChatMembers { get; set; } = new List<ChatMemberEntity>();

    /// <summary>
    /// Сообщения, отправленные пользователем.
    /// </summary>
    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();

    /// <summary>
    /// Звонки, инициированные пользователем.
    /// </summary>
    public ICollection<CallEntity> Calls { get; set; } = new List<CallEntity>();
}