using System.Text;
using System.Threading.Channels;
using _684BakOMeter.Web.Infrastructure.IO;
using _684BakOMeter.Web.Services;

namespace _684BakOMeter.Web.Infrastructure.Protocol;

/// <summary>
/// Subscribes to incoming serial frames from <see cref="ITransport"/>,
/// parses the PREFIX:VALUE protocol, and routes messages to the
/// appropriate application services.
/// </summary>
public class ProtocolClient : IDisposable
{
    private readonly ITransport _transport;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NfcScanBridge _scanBridge;
    private readonly ILogger<ProtocolClient> _logger;

    // Unbounded channel bridges the transport event into an async processing loop
    private readonly Channel<ReadOnlyMemory<byte>> _incoming =
        Channel.CreateUnbounded<ReadOnlyMemory<byte>>(
            new UnboundedChannelOptions { SingleReader = true });

    public ProtocolClient(
        ITransport transport,
        IServiceScopeFactory scopeFactory,
        NfcScanBridge scanBridge,
        ILogger<ProtocolClient> logger)
    {
        _transport = transport;
        _scopeFactory = scopeFactory;
        _scanBridge = scanBridge;
        _logger = logger;
    }

    /// <summary>
    /// Starts the async read loop. Subscribes to the transport's
    /// <see cref="ITransport.FrameReceived"/> event and processes
    /// incoming frames until <paramref name="ct"/> is cancelled.
    /// </summary>
    public async Task RunAsync(CancellationToken ct)
    {
        _transport.FrameReceived += EnqueueFrame;
        _logger.LogInformation("ProtocolClient listening for incoming serial frames.");

        try
        {
            await foreach (var frame in _incoming.Reader.ReadAllAsync(ct))
            {
                await ProcessFrameAsync(frame, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Normal shutdown
        }
        finally
        {
            _transport.FrameReceived -= EnqueueFrame;
            _logger.LogInformation("ProtocolClient stopped listening.");
        }
    }

    /// <summary>Event handler — bridges transport events into the channel.</summary>
    private void EnqueueFrame(object? sender, ReadOnlyMemory<byte> frame)
    {
        _incoming.Writer.TryWrite(frame);
    }

    /// <summary>Decodes a raw frame to a string, parses it, and dispatches.</summary>
    private async Task ProcessFrameAsync(ReadOnlyMemory<byte> frame, CancellationToken ct)
    {
        string line;
        try
        {
            line = Encoding.UTF8.GetString(frame.Span).Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decode incoming frame as UTF-8.");
            return;
        }

        if (string.IsNullOrEmpty(line))
            return;

        if (!ProtocolParser.TryParseLine(line, out var message))
        {
            _logger.LogWarning("Malformed serial message ignored: {Line}", line);
            return;
        }

        try
        {
            await DispatchAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching protocol message {Type}: {Value}",
                message.Type, message.Value);
        }
    }

    /// <summary>Routes a parsed message to the correct service.</summary>
    private async Task DispatchAsync(ProtocolMessage message, CancellationToken ct)
    {
        switch (message.Type)
        {
            case ProtocolMessageType.Rfid:
                await HandleRfidAsync(message.Value, ct);
                break;

            case ProtocolMessageType.Scale:
                await HandleScaleAsync(message.Prefix, message.Value, ct);
                break;

            case ProtocolMessageType.Error:
                _logger.LogWarning("Arduino error: {ErrorMessage}", message.Value);
                break;

            case ProtocolMessageType.System:
                _logger.LogInformation("Arduino system: {SystemMessage}", message.Value);
                break;

            case ProtocolMessageType.Unknown:
                _logger.LogWarning("Unknown protocol prefix '{Prefix}' with value '{Value}'",
                    message.Prefix, message.Value);
                break;
        }
    }

    /// <summary>Resolves a scanned RFID/NFC tag via <see cref="NfcService"/> and
    /// pushes the UID to the frontend poll queue.</summary>
    private async Task HandleRfidAsync(string uid, CancellationToken ct)
    {
        _logger.LogDebug("RFID tag scanned: {Uid}", uid);

        // Push to the poll queue so the frontend NfcScanGate picks it up
        _scanBridge.Enqueue(uid.Trim().ToUpperInvariant());

        await using var scope = _scopeFactory.CreateAsyncScope();
        var nfcService = scope.ServiceProvider.GetRequiredService<NfcService>();
        await nfcService.HandleScannedTagAsync(uid, ct);
    }

    /// <summary>Parses and forwards a scale measurement to <see cref="ChugService"/>.</summary>
    private async Task HandleScaleAsync(string prefix, string rawValue, CancellationToken ct)
    {
        if (!ProtocolParser.TryParseScaleNumber(prefix, out var scaleNumber))
        {
            _logger.LogWarning("Could not determine scale number from prefix: {Prefix}", prefix);
            return;
        }

        if (!ProtocolParser.TryParseDecimal(rawValue, out var value))
        {
            _logger.LogWarning("Malformed scale value '{RawValue}' for {Prefix}", rawValue, prefix);
            return;
        }

        _logger.LogDebug("Scale {ScaleNumber} measurement: {Value}", scaleNumber, value);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var chugService = scope.ServiceProvider.GetRequiredService<ChugService>();
        await chugService.HandleMeasurementAsync(scaleNumber, value, ct);
    }

    public void Dispose()
    {
        _incoming.Writer.TryComplete();
    }
}
