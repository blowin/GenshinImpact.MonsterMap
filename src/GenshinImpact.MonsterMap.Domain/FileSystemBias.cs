using System.Drawing;

namespace GenshinImpact.MonsterMap.Domain;

//calibration
public class FileSystemBias
{
    private const int PixelPerIngIndex = 0;
    private const int PixelPerLatIndex = 1;
    private const int IngBiasIndex = 2;
    private const int LatBiasIndex = 3;
    
    private readonly string _path;
    private readonly AppFloat[] _values;

    public AppFloat PixelPerIng
    {
        get => _values[PixelPerIngIndex];
        set => Update(PixelPerIngIndex, value);
    }
    
    public AppFloat PixelPerLat
    {
        get => _values[PixelPerLatIndex];
        set => Update(PixelPerLatIndex, value);
    }

    public AppFloat IngBias
    {
        get => _values[IngBiasIndex];
        set => Update(IngBiasIndex, value);
    }

    public AppFloat LatBias
    {
        get => _values[LatBiasIndex];
        set => Update(LatBiasIndex, value);
    }


    public FileSystemBias(string path)
    {
        _path = path;
        var configs = File.ReadAllLines(path);
        _values = new AppFloat[4];
        _values[PixelPerIngIndex] = new AppFloat(configs[PixelPerIngIndex]);
        _values[PixelPerLatIndex] = new AppFloat(configs[PixelPerLatIndex]);
        _values[IngBiasIndex] = new AppFloat(configs[IngBiasIndex]);
        _values[LatBiasIndex] = new AppFloat(configs[LatBiasIndex]);
    }

    public int ToMapPosX(int ing, Rectangle targetRect, Size size)
    {
        int x = (int)Math.Round((ing * PixelPerIng * 0.1f + IngBias));
        return (int)((x - targetRect.X) * (size.Width * 1.0f / targetRect.Width));
    }

    public int ToMapPosY(int lat, Rectangle targetRect, Size size)
    {
        int y = (int)Math.Round((lat * PixelPerLat * 0.1f + LatBias));
        //Because it is a wiki map ball, a mapping from a straight line to a curved surface is required
        var scale =lat==0?1: (lat * Math.PI) / (180 * Math.Sin(lat / 180 * Math.PI));
        y=(int)(y*scale);
        return (int)((y - targetRect.Y) * (size.Height * 1.0f / targetRect.Height));
    }
    
    private void Update(int index, float value)
    {
        _values[index] = value;
        File.WriteAllLines(_path, _values.Select(e => e.ToString()));
    }
}