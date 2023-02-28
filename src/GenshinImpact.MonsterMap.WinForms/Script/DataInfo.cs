using System.Drawing;
using System.Windows.Forms;

namespace GenshinImpact.MonsterMap.Script;

/// <summary>
/// Online map detection class
/// </summary>
class DataInfo
{
    public static readonly Bitmap MainMap = (Bitmap)Image.FromFile("img/MainMap.jpg");
    public static Bitmap GameMap;
    public static PictureBox SampleImage; //Sample screenshots from the game
    public static PictureBox PointImage; //Feature point comparison screenshot
    
    public static int Width = 1920;
    public static int Height = 1080;
    public static bool IsDetection = false;
    public static bool IsShowLine = false;
    public static bool IsPauseShowIcon = false;
}