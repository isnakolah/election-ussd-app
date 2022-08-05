using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using USSDApp.Data;
using USSDTest.DTOs;

namespace USSDApp.Services;

public enum Candidate
{
    POLYCARP_IGATHE,
    JOHNSON_SAKAJA,
    AGNES_KAGURE,
    NANCY_MWADIME,
    KENNETH_NYAMWAMU,
    CLEOPAS_MUTUA,
    HARMAN_GREWAL,
    ESTHER_THAIRU,
    DENISE_KODHE
}

public class USSDService
{
    private const int ENTER_RESULTS = 1;
    private const int VIEW_RESULTS = 2;

    private readonly SessionService _sessionService;
    private readonly ResultsService _resultsService;
    private readonly AgentsService _agentsService;
    private readonly ILogger<USSDService> _logger;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public USSDService(ResultsService resultsService, AgentsService agentsService, SessionService sessionService,
        ILogger<USSDService> logger, AppDbContext context, IMemoryCache cache)
    {
        (_resultsService, _agentsService, _sessionService, _logger, _context, _cache) =
            (resultsService, agentsService, sessionService, logger, context, cache);
    }

    public async ValueTask<string> GetOptionsAsync(USSDRequest request)
    {
        var currentStage = _sessionService.GetCurrentStage(request.SessionId);

        _logger.LogInformation("The current stage is {Stage}", currentStage.ToString());

        var response = currentStage switch
        {
            Stage.None => GetFirstView(request.SessionId),
            Stage.PreWelcomeView => await GetWelcomeViewAsync(request.SessionId, request.MSISDN),
            Stage.WelcomeView => await GetResultsViewOrAddResultsView(request.SessionId, request.MSISDN,
                request.Input ?? 0),
            Stage.SelectViewOrAdd => await GetResultsViewOrAddResultsView(request.SessionId, request.MSISDN,
                request.Input ?? 0),
            Stage.SelectPollingStation => await GetSelectPollingStationViewAsync(request.SessionId, request.MSISDN),
            Stage.ViewResults => await GetViewResultsViewAsync(request.SessionId, request.Input ?? 0),
            _ => await GetAddResultsViewAsync(request.SessionId, request.MSISDN, request.Input ?? 0),
        };

        return response.Text;
    }

    private USSDResponse GetFirstView(string sessionId)
    {
        _sessionService.SetStage(sessionId, Stage.PreWelcomeView);

        return new USSDResponse().AddMessage("To continue select 1").AddOption("1", "Continue");
    }

    private async ValueTask<USSDResponse> GetWelcomeViewAsync(string sessionId, string phoneNumber)
    {
        var agentDetails = await _agentsService.GetAgentDetailsFromPhoneNumberAsync(phoneNumber);

        if (agentDetails is null)
            return new USSDResponse().AddMessage("You are not authorized to use this app").EndSession();

        var response = new USSDResponse()
            .AddMessage($"Welcome {agentDetails.UserName}")
            .AddMessage($"Polling Center: {agentDetails.PollingCenterName}")
            .AddOption(ENTER_RESULTS.ToString(), "Enter results")
            .AddOption(VIEW_RESULTS.ToString(), "View results");

        _sessionService.SetStage(sessionId, Stage.SelectViewOrAdd);

        return response;
    }

    private async ValueTask<USSDResponse> GetResultsViewOrAddResultsView(string sessionId, string phoneNumber, int input)
    {
        return input switch
        {
            VIEW_RESULTS => await GetSelectPollingStationViewAsync(sessionId, phoneNumber, false),
            ENTER_RESULTS => await GetSelectPollingStationViewAsync(sessionId, phoneNumber),
            _ => USSDResponse.InvalidInput
        };
    }

    private async ValueTask<USSDResponse> GetSelectPollingStationViewAsync(string sessionId, string phoneNumber, bool addResults = true)
    {
        _sessionService.SetStage(sessionId, addResults ? Stage.AddingResultsOne : Stage.ViewResults);

        var agentDetails = await _agentsService.GetAgentDetailsFromPhoneNumberAsync(phoneNumber);

        var pollingStations = await _context.PollingCenters
            .Where(pc => pc.Id == agentDetails.PollingCenterId)
            .Include(pc => pc.PollingStations)
                .ThenInclude(ps => ps.BaseEntity)
            .SelectMany(ps => ps.PollingStations)
            .ToArrayAsync();

        var response = new USSDResponse().AddMessage($"Select polling station to {(addResults ? "add" : "view")} results");

        foreach (var (index, pollingStation) in pollingStations.Select((ps, i) => (i, ps)))
            response.AddOption((index + 1).ToString(), $"Station {pollingStation.Code[^2..]}");

        return response;
    }

    private async ValueTask<USSDResponse> GetAddResultsViewAsync(string sessionId, string phoneNumber, int input)
    {
        var currentStage = _sessionService.GetCurrentStage(sessionId);

        if (currentStage == Stage.AddingResultsOne)
            _sessionService.SetSelectedPollingStation(sessionId, await GetInputedPollingStationId(input, phoneNumber));

        _sessionService.TryGetSelectedPollingStation(sessionId, out var selectedPollingStation);

        if (currentStage is Stage.AddingResultsOne && await _resultsService.ResultsExistsAsync(selectedPollingStation))
            return new USSDResponse().AddMessage("Results for this station already added").EndSession();

        var response = new USSDResponse();

        var agent = await _agentsService.GetAgentDetailsFromPhoneNumberAsync(phoneNumber);

        var setSession = (Stage _stage) => _sessionService.SetStage(sessionId, _stage);

        var addPartialResults = (Candidate candidate) =>
            _resultsService.AddPartialResults(sessionId, candidate, selectedPollingStation, input);

        switch (currentStage)
        {
            case Stage.SelectViewOrAdd:
                setSession(Stage.AddingResultsOne);
                goto case Stage.AddingResultsOne;
            case Stage.AddingResultsOne:
                response.AddMessage("Enter results for Polycarp Igathe");
                setSession(Stage.AddingResultsTwo);
                break;
            case Stage.AddingResultsTwo:
                addPartialResults(Candidate.POLYCARP_IGATHE);
                response.AddMessage("Enter results for Johnson Sakaja");
                setSession(Stage.AddingResultsThree);
                break;
            case Stage.AddingResultsThree:
                addPartialResults(Candidate.JOHNSON_SAKAJA);
                response.AddMessage("Enter results for Agnes Kagure");
                setSession(Stage.AddingResultsFour);
                break;
            case Stage.AddingResultsFour:
                addPartialResults(Candidate.AGNES_KAGURE);
                response.AddMessage("Enter results for Nancy Mwadime");
                setSession(Stage.AddingResultsFive);
                break;
            case Stage.AddingResultsFive:
                addPartialResults(Candidate.NANCY_MWADIME);
                response.AddMessage("Enter results for Kenneth Nyamwamu");
                setSession(Stage.AddingResultsSix);
                break;
            case Stage.AddingResultsSix:
                addPartialResults(Candidate.KENNETH_NYAMWAMU);
                response.AddMessage("Enter results for Cleopas Mutua");
                setSession(Stage.AddingResultsSeven);
                break;
            case Stage.AddingResultsSeven:
                addPartialResults(Candidate.CLEOPAS_MUTUA);
                response.AddMessage("Enter results for Harman Grewal");
                setSession(Stage.AddingResultsEight);
                break;
            case Stage.AddingResultsEight:
                addPartialResults(Candidate.HARMAN_GREWAL);
                response.AddMessage("Enter results for Esther Thairu");
                setSession(Stage.AddingResultsNine);
                break;
            case Stage.AddingResultsNine:
                addPartialResults(Candidate.ESTHER_THAIRU);
                response.AddMessage("Enter results for Denise Kodhe");
                setSession(Stage.AddingResultsTen);
                break;
            case Stage.AddingResultsTen:
                addPartialResults(Candidate.DENISE_KODHE);
                response.AddMessage("Enter disputed votes results");
                setSession(Stage.AddingResultsEleven);
                break;
            case Stage.AddingResultsEleven:
                _resultsService.AddPartialResultsDisputedSpoiltVotes(sessionId, agent!.PollingCenterId, input);
                response.AddMessage("Enter spoilt votes results");
                setSession(Stage.FinalStage);
                break;
            case Stage.FinalStage:
                _resultsService.AddPartialSpoiltVotes(sessionId, selectedPollingStation, input);
                await _resultsService.PersistResultsAsync(sessionId, selectedPollingStation, phoneNumber);
                response.AddMessage("Results saved successfully").EndSession();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return response;
    }

    private async Task<Guid> GetInputedPollingStationId(int input, string session)
    {
        var key = $"polling-station-{session}";

        if (_cache.TryGetValue(key, out Guid cachedPollingStationId))
            return cachedPollingStationId;

        var pollingStationId = await _context.PollingStations
            .Where(p => p.Code.EndsWith($"0{input}"))
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        return _cache.Set(key, pollingStationId);
    }

    private async ValueTask<USSDResponse> GetViewResultsViewAsync(string sessionId, int input)
    {
        _sessionService.SetStage(sessionId, Stage.ViewResults);

        var response = new USSDResponse();

        var selectedPollingStation = await GetInputedPollingStationId(input, sessionId);

        var (candidates, total) = await _resultsService.GetCandidatesResultsAsync(selectedPollingStation);

        if (total == 0)
            return response.AddMessage("No results yet").EndSession();

        response.AddMessage("The results are").AddEndLine();

        foreach (var result in candidates)
            response.AddMessage($"{result.CandidateName}: {result.Votes}");

        response.AddEndLine().AddMessage($"Total: {total}");

        return response.EndSession();
    }
}