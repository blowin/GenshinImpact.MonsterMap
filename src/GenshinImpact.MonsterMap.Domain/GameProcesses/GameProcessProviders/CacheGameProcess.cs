using System.Runtime.Caching;

namespace GenshinImpact.MonsterMap.Domain.GameProcesses.GameProcessProviders;

public sealed class CacheGameProcess : IGameProcessProvider
{
    private const string CacheKey = "CACHE_GAME_PROCESS";
    private readonly IGameProcessProvider _root;
    private readonly object _lockObj = new object();

    public CacheGameProcess(IGameProcessProvider root)
    {
        _root = root;
    }
    
    public IGameProcess GetProcess()
    {
        lock (_lockObj)
        {
            var cacheItem = MemoryCache.Default.GetCacheItem(CacheKey);
            if (cacheItem?.Value != null)
            {
                var cachedProcess = (IGameProcess)cacheItem.Value;
                if (cachedProcess is not EmptyGameProcess && !cachedProcess.HasExited)
                    return cachedProcess;

                MemoryCache.Default.Remove(CacheKey);
            }

            var process = _root.GetProcess();

            var absoluteExpiration = process.MainWindowHandle == IntPtr.Zero
                ? DateTimeOffset.UtcNow.AddSeconds(2)
                : DateTimeOffset.UtcNow.AddMinutes(5);
        
            MemoryCache.Default.Add(CacheKey, process, absoluteExpiration);
            return process;   
        }
    }
}