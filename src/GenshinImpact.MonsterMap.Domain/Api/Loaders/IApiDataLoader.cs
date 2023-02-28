using GenshinImpact.MonsterMap.Domain.MapMarkers;

namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public interface IApiDataLoader
{
    IEnumerable<MapMarker> Load();
}