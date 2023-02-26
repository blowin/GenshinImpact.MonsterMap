using GenshinImpact.MonsterMap.Domain.Icons;

namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public sealed class CacheApiDataLoader : IApiDataLoader
{
    private readonly Lazy<Icon[]> _lazyLoad;

    public CacheApiDataLoader(IApiDataLoader root)
    {
        _lazyLoad = new Lazy<Icon[]>(() => root.Load().ToArray());
    }

    public IEnumerable<Icon> Load() => _lazyLoad.Value;
}