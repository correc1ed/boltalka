using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Enums.Call;
using boltalka.Application.Models.Call;
using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class CallRepository : BaseRepository<CallEntity, Call>, ICallRepository
{
    public CallRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }

    public async Task<Call?> GetActiveCallAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var resultCall = await _dbContext.Calls
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.ChatId == chatId &&
                (c.Status == (Enum.CallStatus)CallStatus.Pending || c.Status == (Enum.CallStatus)CallStatus.Active),
                cancellationToken);
        
        return _mapper.Map<Call>(resultCall);
    }

    public async Task<IEnumerable<Call>> GetCallHistoryAsync(Guid chatId, int skip, int take, CancellationToken cancellationToken)
    {
        var resultCals = await _dbContext.Calls
            .AsNoTracking()
            .Where(c => c.ChatId == chatId)
            .OrderByDescending(c => c.StartedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<Call>>(resultCals);
    }

    public async Task UpdateStatusAsync(Guid callId, CallStatus status, CancellationToken cancellationToken, DateTime? endedAt = null)
    {
        var call = await _dbContext.Calls.FindAsync(callId, cancellationToken);
        
        if (call is not null)
        {
            call.Status = (Enum.CallStatus)status;
            if (endedAt.HasValue)
            {
                call.EndedAt = endedAt.Value;
            }
            _dbContext.Calls.Update(call);
        }
    }
}