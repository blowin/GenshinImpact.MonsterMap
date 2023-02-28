using System.Drawing;

namespace GenshinImpact.MonsterMap.Domain;

public class GameSize
{
    public int Width { get; set; } = 1920;
    
    public int Height { get; set; } = 1080;

    public Size Size => new Size(Width, Height);
}