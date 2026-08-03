using boltalka.Application.Abstractions.Mappers;
using boltalka.Application.Models.Message;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Mappers;

public class LightMapper : MappingProfile
{
    public LightMapper()
    {
        // ========== Сообщения ==========
        CreateMap<MessageEntity, Application.Models.Message.Message>()
            .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => 
                src.Sender == null ? null : new Application.Models.User.User { Id = src.Sender.Id, DisplayName = src.Sender.DisplayName }))
            .ForMember(dest => dest.Chat, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentUrl, opt => opt.MapFrom(src => 
                src.MessageMediaLinks.Select(mm => mm.Media).Select(m => new Application.Models.Media.Media { Id = m.Id, FileName = m.FileName, ContentType = m.ContentType, StoragePath = m.StoragePath })));

        CreateMap<SendMessage, MessageEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SenderId, opt => opt.Ignore())
            .ForMember(dest => dest.Chat, opt => opt.Ignore())
            .ForMember(dest => dest.Sender, opt => opt.Ignore())
            .ForMember(dest => dest.MessageMediaLinks, opt => opt.Ignore());

        // ========== Медиа ==========
        CreateMap<MediaEntity, Application.Models.Media.Media>()
            .ForMember(dest => dest.UploadedBy, opt => opt.Ignore());  // игнорируем, чтобы не было рекурсии

        // ========== Пользователи (только для тестов, без навигаций) ==========
        CreateMap<UserEntity, Application.Models.User.User>()
            .ForMember(dest => dest.ChatMembers, opt => opt.Ignore())
            .ForMember(dest => dest.Messages, opt => opt.Ignore())
            .ForMember(dest => dest.Calls, opt => opt.Ignore())
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => 
                src.Avatar == null ? null : new Application.Models.Media.Media { Id = src.Avatar.Id, FileName = src.Avatar.FileName, StoragePath = src.Avatar.StoragePath }));

        CreateMap<Application.Models.User.User, UserEntity>()
            .ForMember(dest => dest.ChatMembers, opt => opt.Ignore())
            .ForMember(dest => dest.Messages, opt => opt.Ignore())
            .ForMember(dest => dest.Calls, opt => opt.Ignore())
            .ForMember(dest => dest.Avatar, opt => opt.Ignore());

        // ========== Чаты (базово, без Members и LastMessage) ==========
        CreateMap<ChatEntity, Application.Models.Chat.Chat>()
            .ForMember(dest => dest.Members, opt => opt.Ignore())
            .ForMember(dest => dest.Messages, opt => opt.Ignore());
    }
}