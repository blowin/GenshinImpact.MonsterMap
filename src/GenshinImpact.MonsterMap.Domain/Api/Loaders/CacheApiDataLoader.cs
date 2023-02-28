using GenshinImpact.MonsterMap.Domain.MapMarkers;

namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public sealed class CacheApiDataLoader : IApiDataLoader
{
    private readonly Lazy<MapMarker[]> _lazyLoad;

    public CacheApiDataLoader(IApiDataLoader root)
    {
        _lazyLoad = new Lazy<MapMarker[]>(() => root.Load().ToArray());
    }

    public IEnumerable<MapMarker> Load() => _lazyLoad.Value;
}