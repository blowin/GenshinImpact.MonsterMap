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

        var filePositions = JsonSerializer.Deserialize<List<Icon>>(File.ReadAllText(iconPositionPath));
        if(filePositions != null)
            ReplacePositions(filePositions);
    }

    private List<Icon> GetAllPos { get; } = new();

    public string[] GetIconNames()
    {
        return GetAllPos.Select(e => e.Name).Distinct().ToArray();
    }

    public IEnumerable<Icon> GetIcons(ICollection<string> names) => GetAllPos.Where(pos => names.Contains(pos.Name));

    public void UpdateData()
    {
        var newPositions = _apiDataLoader.Load().ToList();
        ReplacePositions(newPositions);
        File.WriteAllText(_iconPositionPath, JsonSerializer.Serialize(GetAllPos, JsonSerializerOptions));
    }

    private void ReplacePositions(ICollection<Icon> newPositions)
    {
        GetAllPos.Clear();
        GetAllPos.AddRange(newPositions);
    }
}