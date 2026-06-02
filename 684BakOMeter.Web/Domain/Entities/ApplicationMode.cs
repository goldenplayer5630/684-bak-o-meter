namespace _684BakOMeter.Web.Domain.Entities;

/// <summary>
/// The active application environment. Persisted in the <see cref="AppSetting"/> table.
/// Each mode is its own scoring environment — leaderboards are filtered by this value.
/// Modes are activated by typing the corresponding key sequence anywhere in the UI.
/// <list type="bullet">
///   <item><term>Official</term><description>Normal NFC-authenticated play (default).</description></item>
///   <item><term>Dms</term><description>DMS/demos mode — D→M→S. Players type their name.</description></item>
///   <item><term>Diabolo</term><description>Diabolo mode — 6→6→6. Black + red theme.</description></item>
///   <item><term>Lion</term><description>KLC/Lion mode — K→L→C. Deep red + leeuwgod watermark.</description></item>
///   <item><term>Dysis</term><description>Dysis mode — 4→2→7.</description></item>
///   <item><term>Kwaak</term><description>Kwaak mode — K→W→A→A→K.</description></item>
///   <item><term>Kompas</term><description>Kompas mode — K→O→M→P→A→S.</description></item>
///   <item><term>Klootviool</term><description>Klootviool mode — K→L→O→O→T→V→I→O→O→L.</description></item>
/// </list>
/// </summary>
public enum ApplicationMode
{
    Official   = 0,
    Dms        = 1,
    Diabolo    = 2,
    Lion       = 3,
    Dysis      = 4,
    Kwaak      = 5,
    Kompas     = 6,
    Klootviool = 7,
}
