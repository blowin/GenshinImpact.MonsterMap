using System.Diagnostics;
using System.Runtime.Caching;

namespace GenshinImpact.MonsterMap.Domain.GameProcesses;

public sealed class GameProcess : IGameProcess
{
    private readonly Process _process;

    public GameProcess(Process process)
    {
        _process = process;
    }

    public IntPtr MainWindowHandle
    {
        get
        {
            // HACK: TODO
            const string key = "MAIN_HANDLE";
            if (_process.HasExited)
            {
                MemoryCache.Default.Remove(key);
                return IntPtr.Zero;
            }
            
            var cacheHandle = MemoryCache.Default.GetCacheItem(key);
            if (cacheHandle?.Value != null) 
                return (IntPtr)cacheHandle.Value;
            
            var handle = _process.MainWindowHandle;
            if (handle == IntPtr.Zero)
            {
                _process.Refresh();
                handle = _process.MainWindowHandle;
            }

            if (handle == IntPtr.Zero)
                return IntPtr.Zero;

            MemoryCache.Default.Add(key, handle, DateTimeOffset.MaxValue);
            return handle;

        }
    }

    public bool IsTopOfProcess
    {
        get
        {
            var mainHandle = MainWindowHandle;
            if (mainHandle == IntPtr.Zero)
                return false;

            return Win32Api.GetForegroundWindow() == mainHandle;
        }
    }

    public bool HasExited => _process.HasExited;
}