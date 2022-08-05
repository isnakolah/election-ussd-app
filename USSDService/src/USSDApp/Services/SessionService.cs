using Microsoft.Extensions.Caching.Memory;
using USSDApp.Common.Constants;

namespace USSDApp.Services;

public enum Stage
{
    None,
    PreWelcomeView,
    WelcomeView,
    ViewResults,
    SelectViewOrAdd,
    SelectPollingStation,
    AddingResultsOne,
    AddingResultsTwo,
    AddingResultsThree,
    AddingResultsFour,
    AddingResultsFive,
    AddingResultsSix,
    AddingResultsSeven,
    AddingResultsEight,
    AddingResultsNine,
    AddingResultsTen,
    AddingResultsEleven,
    FinalStage,
}

public class SessionService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<SessionService> _logger;

    public SessionService(IMemoryCache cache, ILogger<SessionService> logger)
    {
        (_cache, _logger) = (cache, logger);
    }

    public Stage GetCurrentStage(string sessionId)
    {
        var currentStage = _cache.GetOrCreate(CacheKeys.Session(sessionId),_ => Stage.None);

        _logger.LogInformation("Current stage is {Stage}", currentStage.ToString());
        
        return currentStage;
    }

    public bool TryGetSelectedPollingStation(string sessionId, out Guid selectedPollingStation)
    {
        return _cache.TryGetValue(CacheKeys.SelectedPollingStation(sessionId), out selectedPollingStation);
    }

    public void SetSelectedPollingStation(string sessionId, Guid pollingStation)
    {
        _cache.Set(CacheKeys.SelectedPollingStation(sessionId), pollingStation);
    }

    public Stage SetStage(string sessionId, Stage stage)
    {
        return _cache.Set(CacheKeys.Session(sessionId), stage);
    }
}