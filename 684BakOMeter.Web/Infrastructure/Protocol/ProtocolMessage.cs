namespace _684BakOMeter.Web.Infrastructure.Protocol;

/// <summary>A parsed serial protocol message in PREFIX:VALUE format.</summary>
public record ProtocolMessage(ProtocolMessageType Type, string Prefix, string Value);
