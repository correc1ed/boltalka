using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models;
using boltalka.Application.Models.User;

namespace boltalka.Application.UseCases.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMediaRepository _mediaRepository ;
    private readonly IMapper _mapper;
    
    public UserService(
        IUserRepository userRepository,
        IMediaRepository mediaRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mediaRepository = mediaRepository;
        _mapper = mapper;
    }
    
    public async Task<Result<UserProfile>> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetFullInfoByIdAsync(userId, cancellationToken);
        
        if (user is null)
            return Result<UserProfile>.Failure("Пользователь не найден.");

        return Result<UserProfile>.Success(_mapper.Map<UserProfile>(user));
    }

    public async Task<Result<UserProfile>> UpdateDisplayNameAsync(Guid userId, string newDisplayName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            return Result<UserProfile>.Failure("Имя не может быть пустым.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserProfile>.Failure("Пользователь не найден.");

        user.DisplayName = newDisplayName;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserProfile>.Success(_mapper.Map<UserProfile>(user));
    }

    public async Task<Result<UserProfile>> SetAvatarAsync(Guid userId, Guid mediaId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserProfile>.Failure("Пользователь не найден.");

        var media = await _mediaRepository.GetByIdAsync(mediaId, cancellationToken);
        if (media is null)
            return Result<UserProfile>.Failure("Медиафайл не найден.");

        if (!media.ContentType.StartsWith("image/"))
            return Result<UserProfile>.Failure("Аватаром может быть только изображение.");

        user.AvatarMediaId = mediaId;
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        
        var updatedUser = await _userRepository.GetFullInfoByIdAsync(user.Id, cancellationToken);
        
        return Result<UserProfile>.Success(_mapper.Map<UserProfile>(updatedUser));
    }

    public async Task<Result<IEnumerable<UserProfile>>> SearchUsersAsync(string query, int skip, int take, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Result<IEnumerable<UserProfile>>.Failure("Запрос не может быть пустым.");

        var users = await _userRepository.SearchUsersAsync(query, skip, take, cancellationToken);
        var profiles = _mapper.Map<IEnumerable<UserProfile>>(users);
        
        return Result<IEnumerable<UserProfile>>.Success(profiles);
    }

    public async Task<Result<UserProfile>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user is null)
            return Result<UserProfile>.Failure("Пользователь не найден.");

        return Result<UserProfile>.Success(_mapper.Map<UserProfile>(user));
    }

    public async Task<Result> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure("Пользователь не найден.");

        if (!user.IsActive)
            return Result.Failure("Пользователь уже деактивирован.");

        user.IsActive = false;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ActivateUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure("Пользователь не найден.");

        if (user.IsActive)
            return Result.Failure("Пользователь уже активен.");

        user.IsActive = true;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return Result.Success();
    }
}