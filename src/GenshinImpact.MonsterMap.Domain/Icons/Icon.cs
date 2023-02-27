using System.Text.Json.Serialization;

namespace GenshinImpact.MonsterMap.Domain.Icons;

public class Icon
{
    [JsonPropertyName("name")]
    public string Name { get; private set; }

    [JsonPropertyName("lng")]
    public float Lng { get; private set; }
    
    [JsonPropertyName("lat")]
    public float Lat { get; private set; }
 
    public Icon(string name, float lng, float lat)
    {
        Name = name;
        Lng = lng;
        Lat = lat;
    }

    public int GetX(float pixelPerIng, float ingBias)
    {
        return (int)Math.Round(Lng * pixelPerIng * 0.1f + ingBias);
    }
    
    public int GetY(float pixelPerLat, float latBias)
    {
        return (int)Math.Round(Lat * pixelPerLat * 0.1f + latBias);
    }
}