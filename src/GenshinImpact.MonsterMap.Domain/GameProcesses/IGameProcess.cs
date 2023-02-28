namespace GenshinImpact.MonsterMap.Domain.GameProcesses;

public interface IGameProcess
{
    IntPtr MainWindowHandle { get; }
    bool IsTopOfProcess { get; }
    bool HasExited { get; }
}