using Microsoft.AspNetCore.SignalR;

namespace _684BakOMeter.Web.Hubs;

/// <summary>
/// SignalR hub for real-time chug session updates.
/// All messages are pushed from <see cref="Services.ChugService"/> via
/// <see cref="IHubContext{ChugHub}"/>; no client-to-server methods are needed.
/// 
/// Events pushed to clients:
/// <list type="bullet">
///   <item><c>ChugUpdate</c>  — periodic weight / state snapshot</item>
///   <item><c>ChugStarted</c> — timer started (glass lifted)</item>
///   <item><c>ChugCompleted</c> — timer stopped (glass empty, includes final duration)</item>
/// </list>
/// </summary>
public class ChugHub : Hub;
