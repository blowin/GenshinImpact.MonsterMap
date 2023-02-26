using GenshinImpact.MonsterMap.Domain.Api.Loaders;
using Newtonsoft.Json;

namespace GenshinImpact.MonsterMap.Domain.Icons;

public class IconPositionProvider
{
    private readonly IApiDataLoader _apiDataLoader;
    private readonly string _iconPositionPath;

    public IconPositionProvider(IApiDataLoader apiDataLoader, string iconPositionPath)
    {
        _iconPositionPath = iconPositionPath;
        _apiDataLoader = apiDataLoader;

        var filePositions = JsonConvert.DeserializeObject<List<Icon>>(File.ReadAllText(iconPositionPath));
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
        File.WriteAllText(_iconPositionPath, JsonConvert.SerializeObject(GetAllPos, Formatting.Indented));
    }

    private void ReplacePositions(ICollection<Icon> newPositions)
    {
        GetAllPos.Clear();
        GetAllPos.AddRange(newPositions);
    }
}