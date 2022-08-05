using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using USSDApp.Common.Constants;
using USSDApp.Data;

namespace USSDApp.Services;

public record AgentDetails
{
    public string PollingCenterName { get; init; } = string.Empty;
    public Guid PollingCenterId { get; init; }
    public string UserName { get; set; } = string.Empty;
}

public class AgentsService
{
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _context;
    private readonly UserService _userService;

    public AgentsService(IMemoryCache cache, AppDbContext context, UserService userService)
    {
        (_cache, _context, _userService) = (cache, context, userService);
    }

    public async ValueTask<AgentDetails?> GetAgentDetailsFromPhoneNumberAsync(string phoneNumber)
    {
        if (_cache.TryGetValue(CacheKeys.AgentDetails(phoneNumber), out AgentDetails cachedAgentDetails))
            return cachedAgentDetails;

        if (await _userService.GetUserFromPhoneNumber(phoneNumber) is not {} user)
            return null;
 
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == user.Id);

        if (agent is null)
            return null;
        
        var pollingStation = await _context.PollingStations.FirstOrDefaultAsync(p => p.Id == agent.PollingStationId);

        var agentDetails = new AgentDetails
        {
            PollingCenterName = pollingStation?.Name ?? string.Empty,
            PollingCenterId = agent.PollingCenterId,
            UserName = $"{user.FirstName} {user.LastName}"
        };

        return _cache.Set(CacheKeys.AgentDetails(phoneNumber), agentDetails, TimeSpan.FromSeconds(90));
    }
}