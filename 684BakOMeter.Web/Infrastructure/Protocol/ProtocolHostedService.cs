using _684BakOMeter.Web.Infrastructure.IO;

namespace _684BakOMeter.Web.Infrastructure.Protocol;

/// <summary>
/// Background service that opens the serial transport and runs the
/// <see cref="ProtocolClient"/> read loop for the lifetime of the application.
/// </summary>
public class ProtocolHostedService : BackgroundService
{
    private readonly ITransport _transport;
    private readonly ProtocolClient _protocolClient;
    private readonly IConfiguration _config;
    private readonly ILogger<ProtocolHostedService> _logger;

    public ProtocolHostedService(
        ITransport transport,
        ProtocolClient protocolClient,
        IConfiguration config,
        ILogger<ProtocolHostedService> logger)
    {
        _transport = transport;
        _protocolClient = protocolClient;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var portName = _config["Serial:PortName"];
        var baudRate = _config.GetValue("Serial:BaudRate", 115200);

        if (string.IsNullOrWhiteSpace(portName))
        {
            _logger.LogWarning(
                "Serial:PortName is not configured. ProtocolClient will not start. " +
                "Set Serial:PortName in appsettings.json to enable serial communication.");
            return;
        }

        try
        {
            _logger.LogInformation("Opening serial port {Port} at {Baud} baud.", portName, baudRate);
            await _transport.OpenAsync(portName, baudRate);

            // Blocks until cancellation — this keeps the hosted service alive
            await _protocolClient.RunAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProtocolHostedService encountered a fatal error on port {Port}.", portName);
        }
        finally
        {
            _logger.LogInformation("Closing serial port.");
            await _transport.CloseAsync();
        }
    }
}
