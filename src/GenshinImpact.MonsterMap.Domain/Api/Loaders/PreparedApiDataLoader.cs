namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public sealed class PreparedApiDataLoader : IApiDataLoader
{
    private readonly IApiDataLoader[] _apiDataLoaders;

    public PreparedApiDataLoader()
    {
        _apiDataLoaders = new IApiDataLoader[]
        {
            new CacheApiDataLoader(new ApiDataLoader("game=ys&ts=1618932203814&markTypes=87%2C88%2C89%2C105%2C106%2C107%2C108%2C175%2C&sign=5b69e10fcad70f825783527cbd6d0e1c")),
            new CacheApiDataLoader(new ApiDataLoader("game=ys&ts=1619009490581&markTypes=115%2C84%2C90%2C91%2C92%2C93%2C94%2C101%2C102%2C103%2C109%2C110%2C111%2C176%2C178%2C&sign=3c7c722f986faf9f1b8babcebd261070")),
            new CacheApiDataLoader(new ApiDataLoader("game=ys&ts=1619192690124&markTypes=15%2C17%2C18%2C29%2C32%2C37%2C41%2C51%2C180%2C&sign=efca01169d0c6326851ee09d19bb9432")),
            new CacheApiDataLoader(new ApiDataLoader("game=ys&ts=1619103048545&markTypes=24%2C27%2C28%2C33%2C40%2C42%2C53%2C54%2C83%2C181%2C&sign=dfd5178594302b315745b628c261d322")),
            // God pupil
            new CacheApiDataLoader(new ApiDataLoader("game=ys&ts=1619694104528&markTypes=2%2C9%2C76%2C81%2C112%2C&sign=a7f4e42d2c0eafb92704ba76d0d48e29")),
            // the trees
            new CacheApiDataLoader(new ApiDataLoader("game=ys&ts=1620198102608&markTypes=185%2C186%2C187%2C188%2C189%2C190%2C191%2C&sign=df8cc0589b0bbce76477d5326b424d81")),
        };
    }

    public IEnumerable<FileIcon> Load()
    {
        foreach (var apiDataLoader in _apiDataLoaders)
        {
            foreach (var fileIcon in apiDataLoader.Load())
                yield return fileIcon;
        }
    }
}