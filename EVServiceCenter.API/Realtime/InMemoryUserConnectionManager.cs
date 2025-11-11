using System.Collections.Concurrent;

namespace EVServiceCenter.API.Realtime;

/// <summary>
/// Thread-safe in-memory implementation for tracking user SignalR connections.
/// </summary>
public class InMemoryUserConnectionManager : IUserConnectionManager
{
    private readonly ConcurrentDictionary<int, HashSet<string>> _connections = new();

    public void AddConnection(int userId, string connectionId)
    {
        var set = _connections.GetOrAdd(userId, _ => new HashSet<string>());
        lock (set)
        {
            set.Add(connectionId);
        }
    }

    public IReadOnlyCollection<string> GetUserConnections(int userId)
    {
        if (_connections.TryGetValue(userId, out var set))
        {
            lock (set)
            {
                return set.ToArray();
            }
        }

        return Array.Empty<string>();
    }

    public void RemoveConnection(int userId, string connectionId)
    {
        if (_connections.TryGetValue(userId, out var set))
        {
            lock (set)
            {
                if (set.Remove(connectionId) && set.Count == 0)
                {
                    _connections.TryRemove(userId, out _);
                }
            }
        }
    }
}
