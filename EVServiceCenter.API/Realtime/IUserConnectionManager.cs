namespace EVServiceCenter.API.Realtime;

/// <summary>
/// Tracks SignalR connection IDs for authenticated users so we can target them outside of hubs.
/// </summary>
public interface IUserConnectionManager
{
    void AddConnection(int userId, string connectionId);
    void RemoveConnection(int userId, string connectionId);
    IReadOnlyCollection<string> GetUserConnections(int userId);
}
