using GenshinImpact.MonsterMap.Domain.Icons;

namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public interface IApiDataLoader
{
    IEnumerable<Icon> Load();
}