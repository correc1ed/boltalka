using boltalka.Application.Abstractions.Mappers;

namespace boltalka.Application.Mappers.User;

public class RegisterToUserMapper : MappingProfile
{
    public RegisterToUserMapper()
    {
        CreateMap<Application.Models.Auth.Register, Application.Models.User.User>(
            (source, mapper) => new Application.Models.User.User
            {
                Id = Guid.NewGuid(),
                Login = source.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(source.Password),
                DisplayName = source.DisplayName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            });
    }
}