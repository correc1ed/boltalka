namespace boltalka.Contracts.Models.Chat;

public class CreateGroupChatContract
{
    public string Name { get; set; } = string.Empty; 
    public List<Guid> MemberIds { get; set; } = new();
}