using System.Text.Json;
using GenshinImpact.MonsterMap.Domain.Api.Loaders;

namespace GenshinImpact.MonsterMap.Domain.MapMarkers;

public class MapMarkerProvider
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };
    
    private readonly IApiDataLoader _apiDataLoader;
    private readonly string _iconPositionPath;
    
    public MapMarkerProvider(IApiDataLoader apiDataLoader, string iconPositionPath)
    {
        _iconPositionPath = iconPositionPath;
        _apiDataLoader = apiDataLoader;

        var filePositions = JsonSerializer.Deserialize<List<MapMarker>>(File.ReadAllText(iconPositionPath)) ?? new List<MapMarker>();
        Positions = ToLookup(filePositions);
    }

    private ILookup<string, MapMarker> Positions { get; set; }

    public string[] GetIconNames()
    {
        return Positions.Select(e => e.Key).ToArray();
    }

    public IEnumerable<MapMarker> GetMarkersByIcon(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            if (!Positions.Contains(name)) 
                continue;

            foreach (var icon in Positions[name])
            {
                yield return icon;
            }
        }
    }

    public void UpdateData()
    {
        var newPositions = _apiDataLoader.Load().ToList();
        Positions = ToLookup(newPositions);
        File.WriteAllText(_iconPositionPath, JsonSerializer.Serialize(Positions, JsonSerializerOptions));
    }

    private static ILookup<string, MapMarker> ToLookup(ICollection<MapMarker> newPositions)
    {
        return newPositions.ToLookup(e => e.Name);
    }
}