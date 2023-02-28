namespace GenshinImpact.MonsterMap.Domain.GameProcesses;

public sealed class EmptyGameProcess : IGameProcess
{
    public static readonly IGameProcess Instance = new EmptyGameProcess();
    
    public IntPtr MainWindowHandle => IntPtr.Zero;
    
    public bool IsTopOfProcess => false;
    public bool HasExited => true;

    private EmptyGameProcess(){}
}