namespace USSDTest.DTOs;

public record USSDRequest
{
    public string SessionId { get; init; } = string.Empty;
    public string MSISDN { get; init; } = string.Empty;
    public int? Input { get; init; }

    public void Deconstruct(out string sessionId, out string phoneNumber, out int input)
    {
        (sessionId, phoneNumber, input) = (SessionId, MSISDN, Input ?? 0);
    }
}