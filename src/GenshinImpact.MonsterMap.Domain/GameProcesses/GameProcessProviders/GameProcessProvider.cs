using System.Diagnostics;

namespace GenshinImpact.MonsterMap.Domain.GameProcesses.GameProcessProviders;

public sealed class GameProcessProvider : IGameProcessProvider
{
    private readonly string _searchProcess;

    public GameProcessProvider(string searchProcess)
    {
        _searchProcess = searchProcess;
    }

    public IGameProcess GetProcess()
    {
        var processes = Process.GetProcessesByName(_searchProcess);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (processes == null || processes.Length == 0)
            return EmptyGameProcess.Instance;
        
        foreach (var process in processes)
        {
            if (process.MainWindowHandle == IntPtr.Zero)
                process.Refresh();

            if (process.MainWindowHandle != IntPtr.Zero)
                return new GameProcess(process);
        }

        return new GameProcess(processes[0]);
    }
}