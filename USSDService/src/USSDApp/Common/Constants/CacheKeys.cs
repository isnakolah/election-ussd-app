namespace USSDApp.Common.Constants;

public static class CacheKeys
{
    public static string AgentDetails(string phoneNumber) => $"AgentDetails_{phoneNumber}";
    public static string User(string phoneNumber) => $"User_{phoneNumber}";
    public static string Session(string sessionId) => $"Session_{sessionId}";
    public static string SelectedPollingStation(string sessionId) => $"SelectedPollingStation_{sessionId}";
}