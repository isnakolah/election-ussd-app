using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using USSDApp.Data;
using USSDTest.Models;
using USSDTest.Models.Common;

namespace USSDApp.Services;

public class ResultsService
{
    private readonly UserService _userService;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public ResultsService(IMemoryCache cache, AppDbContext context, UserService userService)
    {
        (_cache, _context, _userService) = (cache, context, userService);
    }

    public async Task<((string CandidateName, int Votes)[], int Total)> GetCandidatesResultsAsync(Guid pollingStationId)
    {
        var results = await _context.Results
            .Where(x => x.PollingStationId == pollingStationId)
            .ToArrayAsync();

        if (!results.Any())
            return (Array.Empty<(string CandidateName, int Votes)>(), 0);

        var total = results.Where(x => x.Votes.HasValue).Sum(x => x.Votes);
        var spoilt = results.First(x => x.Spoilt != null).Spoilt!.Value;

        var disputed = results.First(x => x.Disputed != null).Disputed!.Value;

        total += disputed + spoilt;

        var votes = results
            .Where(x => x.Votes != null)
            .Select(r => (CandidateName: GetCandidateNameFromId(r.CandidateId.ToString()), Votes: r.Votes!.Value))
            .ToList();

        votes.AddRange(new (string CandidateName,int Votes)[]
        {
            ("Disputed", disputed),
            ("Spoilt", spoilt)
        });

        return (votes.ToArray(), total!.Value);
    }

    public async ValueTask<bool> ResultsExistsAsync(Guid pollingStationId)
    {
        return await _context.Results.AnyAsync(x => x.PollingStationId == pollingStationId);
    }

    public void AddPartialResults(string sessionId, Candidate candidate, Guid pollingStationId, int votes)
    {
        AddPartialResults(sessionId, new Result {PollingStationId = pollingStationId, CandidateId = GetCandidateId(candidate), Votes = votes});
    }

    public void AddPartialSpoiltVotes(string sessionId, Guid pollingStationId, int votes)
    {
        AddPartialResults(sessionId, new Result {PollingStationId = pollingStationId, Spoilt = votes});
    }

    public void AddPartialResultsDisputedSpoiltVotes(string sessionId, Guid pollingStationId, int votes)
    {
        AddPartialResults(sessionId, new Result {PollingStationId = pollingStationId, Disputed = votes});
    }

    public async Task PersistResultsAsync(string sessionId, Guid pollingStationId, string phoneNumber)
    {
        var user = await _userService.GetUserFromPhoneNumber(phoneNumber);

        var results = _cache.Get<List<Result>>($"{sessionId}-partial-results");
        
        var candidateResults = results
            .Where(x => x.CandidateId.HasValue)
            .Select(x => new {candidate_uuid = x.CandidateId, votes = x.Votes});

        var body = new
        {
            user_id = user?.Id,
            polling_station_id = pollingStationId,
            results = candidateResults,
            spoilt_votes = results.First(x => x.Spoilt != null).Spoilt,
            disputed_votes = results.First(x => x.Disputed != null).Disputed
        };

        using var httpClient = new HttpClient {BaseAddress = new Uri("http://backend.047nairobi.com")};

        await  httpClient.PostAsJsonAsync("polls/ussdresults/", body);
    }

    private void AddPartialResults(string sessionId, Result result)
    {
        var key = $"{sessionId}-partial-results";

        var sessionPartialResults = _cache.GetOrCreate(key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(1);
            return new List<Result>();
        });

       sessionPartialResults.Add(result);

       _cache.Set(key, sessionPartialResults, TimeSpan.FromMinutes(1));
    }
    
    private static Guid GetCandidateId(Candidate candidate)
    {
        var candidateId = candidate switch
        {
            Candidate.POLYCARP_IGATHE => new Guid("70416b70-3b8a-497b-81b5-8bc1242052aa"),
            Candidate.JOHNSON_SAKAJA => new Guid("e25806b1-8723-4aec-b7ac-0db82d21927a"),
            Candidate.AGNES_KAGURE => new Guid("d2e13315-1b42-4cf2-ac4f-0cb5220ab73c"),
            Candidate.NANCY_MWADIME => new Guid("faaf32f2-5e0d-418a-8474-336ab435f7c6"),
            Candidate.KENNETH_NYAMWAMU => new Guid("353f59b8-bdd3-4dfb-952a-8300de77885b"),
            Candidate.CLEOPAS_MUTUA => new Guid("28414b24-21ee-49ba-a290-178006b9bf75"),
            Candidate.HARMAN_GREWAL => new Guid("5b5ebf2f-b44e-4c62-8163-72a8f4b460b7"),
            Candidate.ESTHER_THAIRU => new Guid("5b5ebf2f-b44e-4c62-8163-72a8f4b460b7"),
            Candidate.DENISE_KODHE => new Guid("2d8fdde4-16a7-4b93-a392-de239988ce91"),
            _ => Guid.Empty
        };

        return candidateId;
    }

    private static string GetCandidateNameFromId(string id)
    {
        Console.WriteLine(id);
        
        var candidateName = id switch
        {
            "70416b70-3b8a-497b-81b5-8bc1242052aa" => "POLYCARP IGATHE",
            "e25806b1-8723-4aec-b7ac-0db82d21927a" => "JOHNSON SAKAJA",
            "d2e13315-1b42-4cf2-ac4f-0cb5220ab73c" => "AGNES KAGURE",
            "faaf32f2-5e0d-418a-8474-336ab435f7c6" => "NANCY MWADIME",
            "353f59b8-bdd3-4dfb-952a-8300de77885b" => "KENNETH NYAMWAMU",
            "28414b24-21ee-49ba-a290-178006b9bf75" => "CLEOPAS MUTUA",
            "5b5ebf2f-b44e-4c62-8163-72a8f4b460b7" => "HARMAN GREWAL",
            "52d738fd-2068-45de-a866-77e27bb80082" => "ESTHER THAIRU",
            "2d8fdde4-16a7-4b93-a392-de239988ce91" => "DENISE KODHE",
            _ => string.Empty
        };

        return candidateName;
        
    }
}