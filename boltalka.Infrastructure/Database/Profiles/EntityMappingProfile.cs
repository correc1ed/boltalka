using AutoMapper;
using boltalka.Application.Models.Call;
using boltalka.Application.Models.Chat;
using boltalka.Application.Models.ChatMember;
using boltalka.Application.Models.Media;
using boltalka.Application.Models.Message;
using boltalka.Application.Models.MessageMedia;
using boltalka.Application.Models.RefreshToken;
using boltalka.Application.Models.User;
using boltalka.Infrastructure.Database.Entities;

namespace boltalka.Infrastructure.Database.Profiles;

public class EntityMappingProfile : Profile
{
    public EntityMappingProfile()
    {
        CreateMap<Call, CallEntity>();
        CreateMap<CallEntity, Call>();
        
        CreateMap<Chat, ChatEntity>();
        CreateMap<ChatEntity, Chat>();
        
        CreateMap<ChatMember, ChatMemberEntity>();
        CreateMap<ChatMemberEntity, ChatMember>();
        
        CreateMap<Media, MediaEntity>();
        CreateMap<MediaEntity, Media>();
        
        CreateMap<Message, MessageEntity>();
        CreateMap<MessageEntity, Message>();
        
        CreateMap<MessageMedia, MessageMediaEntity>();
        CreateMap<MessageMediaEntity, MessageMedia>();
        
        CreateMap<RefreshToken, RefreshTokenEntity>();
        CreateMap<RefreshTokenEntity, RefreshToken>();
        
        CreateMap<User, UserEntity>();
        CreateMap<UserEntity, User>();
    }
}