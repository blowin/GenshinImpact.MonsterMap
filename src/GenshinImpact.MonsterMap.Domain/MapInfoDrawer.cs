using System.Drawing;
using GenshinImpact.MonsterMap.Domain.MapMarkers;

namespace GenshinImpact.MonsterMap.Domain;

public class MapInfoDrawer
{
    private static readonly Pen RedPen = new Pen(new SolidBrush(Color.Red));
    private static readonly Pen WhitePen = new Pen(new SolidBrush(Color.White));
    private static readonly Dictionary<string, Bitmap> IconDict = LoadData();

    private readonly FileSystemBias _bias;
    private readonly Func<IEnumerable<MapMarker>> _iconLoader;
    private readonly GameSize _gameSize;

    public MapInfoDrawer(FileSystemBias bias, GameSize gameSize, Func<IEnumerable<MapMarker>> iconLoader)
    {
        _bias = bias;
        _iconLoader = iconLoader;
        _gameSize = gameSize;
    }

    public void DrawLines(Graphics graphics, Rectangle targetRect)
    {
        for (int x = -100; x < 110; x += 10)
        {
            graphics.DrawLine(WhitePen, _bias.ToMapPosX(x, targetRect, _gameSize.Size),
                _bias.ToMapPosY(-100, targetRect, _gameSize.Size), _bias.ToMapPosX(x, targetRect, _gameSize.Size),
                _bias.ToMapPosY(100, targetRect, _gameSize.Size));
        }

        for (int y = -100; y < 110; y += 10)
        {
            graphics.DrawLine(WhitePen, _bias.ToMapPosX(-100, targetRect, _gameSize.Size),
                _bias.ToMapPosY(y, targetRect, _gameSize.Size), _bias.ToMapPosX(100, targetRect, _gameSize.Size),
                _bias.ToMapPosY(y, targetRect, _gameSize.Size));
        }

        graphics.DrawLine(RedPen, _bias.ToMapPosX(-100, targetRect, _gameSize.Size),
            _bias.ToMapPosY(0, targetRect, _gameSize.Size), _bias.ToMapPosX(100, targetRect, _gameSize.Size),
            _bias.ToMapPosY(0, targetRect, _gameSize.Size));
        graphics.DrawLine(RedPen, _bias.ToMapPosX(0, targetRect, _gameSize.Size),
            _bias.ToMapPosY(-100, targetRect, _gameSize.Size), _bias.ToMapPosX(0, targetRect, _gameSize.Size),
            _bias.ToMapPosY(100, targetRect, _gameSize.Size));
    }

    public void DrawMarkers(Graphics graphics, Rectangle targetRect)
    {
        foreach (var pos in _iconLoader())
        {
            var x = (int)((pos.GetX(_bias.PixelPerIng, _bias.IngBias) - targetRect.X) *
                          (_gameSize.Width * 1.0f / targetRect.Width));
            var y = (int)((pos.GetY(_bias.PixelPerLat, _bias.LatBias) - targetRect.Y) *
                          (_gameSize.Height * 1.0f / targetRect.Height));
            var icon = IconDict[pos.Name];
            var iconWidth = x - icon.Width / 2;
            var iconHeight = y - icon.Height;
            if (iconWidth > 0 && iconHeight > 0 && iconWidth < _gameSize.Width && iconHeight < _gameSize.Height)
            {
                graphics.DrawImage(icon, new PointF(iconWidth, iconHeight));
            }
        }
    }
    
    private static Dictionary<string, Bitmap> LoadData()
    {
        var res = new Dictionary<string, Bitmap>();
        foreach (var icon in new DirectoryInfo("icon").GetFiles())
        {
            res[icon.Name] = (Bitmap)Image.FromFile(icon.FullName);
        }
        return res;
    }
}