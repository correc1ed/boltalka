using System.ComponentModel.DataAnnotations;

namespace boltalka.Infrastructure.Database.Abstract;

/// <summary>
/// Базовая сущность.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}