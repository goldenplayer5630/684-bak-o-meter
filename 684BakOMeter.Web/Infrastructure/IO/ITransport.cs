namespace _684BakOMeter.Web.Infrastructure.IO
{
    public interface ITransport : IAsyncDisposable
    {
        bool IsOpen { get; }
        string PortName { get; }
        Task OpenAsync(string portName, int baud);
        Task CloseAsync();

        // Already-framed bytes
        Task WriteAsync(ReadOnlyMemory<byte> frame, CancellationToken ct = default);

        // Fired by read loop with raw bytes of one complete frame (or pass parsed envelope if you prefer)
        event EventHandler<ReadOnlyMemory<byte>> FrameReceived;
    }
}
