using System.Collections.Concurrent;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Singleton in-memory bridge between the serial protocol layer and the
/// frontend polling endpoint. The <see cref="Infrastructure.Protocol.ProtocolClient"/>
/// enqueues scanned tag UIDs, and the NFC poll API dequeues them.
/// </summary>
public class NfcScanBridge
{
    private readonly ConcurrentQueue<string> _queue = new();

    /// <summary>Pushes a scanned NFC tag UID for the frontend to pick up.</summary>
    public void Enqueue(string uid) => _queue.Enqueue(uid);

    /// <summary>Tries to dequeue the next scanned UID. Returns false if empty.</summary>
    public bool TryDequeue(out string? uid) => _queue.TryDequeue(out uid);
}
