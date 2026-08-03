using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Enums.Call;
using boltalka.Application.Models;
using boltalka.Application.Models.Call;

namespace boltalka.Application.UseCases.Services;

public class CallService : ICallService
{
    private readonly ICallRepository _callRepository;
    private readonly IChatRepository _chatRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    
    public CallService(
        ICallRepository callRepository,
        IChatRepository chatRepository,
        INotificationService notificationService,
        IMapper mapper)
    {
        _callRepository = callRepository;
        _chatRepository = chatRepository;
        _notificationService = notificationService;
        _mapper = mapper;
    }
    
    public async Task<Result<Call>> StartCallAsync(Guid chatId, Guid initiatorId, CallType callType, CancellationToken cancellationToken)
    {
        if (!await _chatRepository.IsUserInChatAsync(chatId, initiatorId, cancellationToken))
            return Result<Call>.Failure("Вы не являетесь участником чата.");
        
        var activeCall = await _callRepository.GetActiveCallAsync(chatId, cancellationToken);
        
        if (activeCall is not null)
            return Result<Call>.Failure("В чате уже есть активный звонок.");
        
        var call = new Call
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            InitiatorId = initiatorId,
            StartedAt = DateTime.UtcNow,
            Status = CallStatus.Pending,
            Type = callType
        };
        
        await _callRepository.AddAsync(call, cancellationToken);

        await _notificationService.NotifyIncomingCallAsync(chatId, _mapper.Map<Call>(call), cancellationToken,  new[] { initiatorId });

        return Result<Call>.Success(_mapper.Map<Call>(call));
    }

    public async Task<Result<Call>> AcceptCallAsync(Guid callId, Guid userId, CancellationToken cancellationToken)
    {
        var call = await _callRepository.GetByIdAsync(callId, cancellationToken);
        if (call is null)
            return Result<Call>.Failure("Звонок не найден.");

        if (call.Status != CallStatus.Pending)
            return Result<Call>.Failure("Нельзя принять звонок: неверный статус.");

        if (!await _chatRepository.IsUserInChatAsync(call.ChatId, userId,  cancellationToken))
            return Result<Call>.Failure("Вы не являетесь участником чата.");

        call.Status = CallStatus.Active;
        await _callRepository.UpdateAsync(call, cancellationToken);

        await _notificationService.NotifyCallStatusChangedAsync(call.ChatId, call, cancellationToken);
        
        return Result<Call>.Success(_mapper.Map<Call>(call));
    }

    public async Task<Result<Call>> EndCallAsync(Guid callId, Guid userId, CancellationToken cancellationToken)
    {
        var call = await _callRepository.GetByIdAsync(callId, cancellationToken);
        if (call is null)
            return Result<Call>.Failure("Звонок не найден.");

        if (call.Status != CallStatus.Active && call.Status != CallStatus.Pending)
            return Result<Call>.Failure("Звонок уже завершён или пропущен.");

        if (!await _chatRepository.IsUserInChatAsync(call.ChatId, userId,  cancellationToken))
            return Result<Call>.Failure("Вы не являетесь участником чата.");

        call.Status = CallStatus.Ended;
        call.EndedAt = DateTime.UtcNow;
        await _callRepository.UpdateAsync(call, cancellationToken);   // сохраняет внутри

        await _notificationService.NotifyCallStatusChangedAsync(call.ChatId, call, cancellationToken);
        
        return Result<Call>.Success(_mapper.Map<Call>(call));
    }

    public async Task<Result> DeclineCallAsync(Guid callId, Guid userId, CancellationToken cancellationToken)
    {
        var call = await _callRepository.GetByIdAsync(callId, cancellationToken);
        if (call is null)
            return Result.Failure("Звонок не найден.");

        if (call.Status != CallStatus.Pending)
            return Result.Failure("Нельзя отклонить звонок: неверный статус.");

        if (!await _chatRepository.IsUserInChatAsync(call.ChatId, userId, cancellationToken))
            return Result.Failure("Вы не являетесь участником чата.");

        if (call.InitiatorId == userId)
            return Result.Failure("Инициатор не может отклонить собственный звонок.");

        call.Status = CallStatus.Missed;
        call.EndedAt = DateTime.UtcNow;
        await _callRepository.UpdateAsync(call, cancellationToken);

        await _notificationService.NotifyCallStatusChangedAsync(call.ChatId, call, cancellationToken);
        
        return Result.Success();
    }

    public async Task<Result<Call?>> GetActiveCallAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var call = await _callRepository.GetActiveCallAsync(chatId, cancellationToken);
        return Result<Call?>.Success(call is not null ? _mapper.Map<Call>(call) : null);
    }

    public async Task<Result<IEnumerable<Call>>> GetCallHistoryAsync(Guid chatId, Guid userId, int skip, int take, CancellationToken cancellationToken)
    {
        if (!await _chatRepository.IsUserInChatAsync(chatId, userId,  cancellationToken))
            return Result<IEnumerable<Call>>.Failure("Вы не являетесь участником чата.");

        var calls = await _callRepository.GetCallHistoryAsync(chatId, skip, take, cancellationToken);
        return Result<IEnumerable<Call>>.Success(_mapper.Map<IEnumerable<Call>>(calls));
    }
}