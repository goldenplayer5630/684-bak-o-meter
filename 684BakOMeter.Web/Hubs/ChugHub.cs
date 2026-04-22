using Microsoft.AspNetCore.SignalR;

namespace _684BakOMeter.Web.Hubs;

/// <summary>
/// SignalR hub for real-time chug session updates.
/// All messages are pushed from <see cref="Services.ChugService"/> via
/// <see cref="IHubContext{ChugHub}"/>; no client-to-server methods are needed.
///
/// Events pushed to clients:
/// <list type="bullet">
///   <item><c>ScaleRaw</c>      — raw scale reading (always, regardless of session state)</item>
///   <item><c>ChugUpdate</c>   — periodic state/weight snapshot</item>
///   <item><c>ChugStarted</c>  — glass lifted, timer started</item>
///   <item><c>ChugCompleted</c>— glass returned and confirmed, includes final duration</item>
/// </list>
/// </summary>
public class ChugHub : Hub;

