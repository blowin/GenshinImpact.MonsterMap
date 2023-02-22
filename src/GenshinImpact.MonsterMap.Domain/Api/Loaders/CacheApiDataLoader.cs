namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public sealed class CacheApiDataLoader : IApiDataLoader
{
    private readonly Lazy<FileIcon[]> _lazyLoad;

    public CacheApiDataLoader(IApiDataLoader root)
    {
        _lazyLoad = new Lazy<FileIcon[]>(() => root.Load().ToArray());
    }

    public IEnumerable<FileIcon> Load() => _lazyLoad.Value;
}