namespace boltalka.Contracts.Models.Message;

public class SendMessageContract
{
    public Guid ChatId { get; set; }
    public string? Text { get; set; }
    public List<Guid> MediaIds { get; set; } = new();
}