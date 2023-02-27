using System.Text.Json;
using System.Xml;
using GenshinImpact.MonsterMap.Domain.Api.Loaders;

namespace GenshinImpact.MonsterMap.Domain.Icons;

public class IconPositionProvider
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };
    
    private readonly IApiDataLoader _apiDataLoader;
    private readonly string _iconPositionPath;
    
    public IconPositionProvider(IApiDataLoader apiDataLoader, string iconPositionPath)
    {
        _iconPositionPath = iconPositionPath;
        _apiDataLoader = apiDataLoader;

        var filePositions = JsonSerializer.Deserialize<List<Icon>>(File.ReadAllText(iconPositionPath)) ?? new List<Icon>();
        ReplacePositions(filePositions);
    }

    private ILookup<string, Icon> GetAllPos { get; set; }

    public string[] GetIconNames()
    {
        return GetAllPos.Select(e => e.Key).ToArray();
    }

    public IEnumerable<Icon> GetIcons(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            if (!GetAllPos.Contains(name)) 
                continue;

            foreach (var icon in GetAllPos[name])
            {
                yield return icon;
            }
        }
    }

    public void UpdateData()
    {
        var newPositions = _apiDataLoader.Load().ToList();
        ReplacePositions(newPositions);
        File.WriteAllText(_iconPositionPath, JsonSerializer.Serialize(GetAllPos, JsonSerializerOptions));
    }

    private void ReplacePositions(ICollection<Icon> newPositions)
    {
        GetAllPos = newPositions.ToLookup(e => e.Name);
    }
}