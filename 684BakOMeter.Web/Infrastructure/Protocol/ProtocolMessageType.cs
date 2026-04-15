namespace _684BakOMeter.Web.Infrastructure.Protocol;

/// <summary>Identifies the kind of message received over the serial protocol.</summary>
public enum ProtocolMessageType
{
    Rfid,
    Scale,
    Error,
    System,
    Unknown
}
