using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Caching;
using System.Windows.Forms;

namespace GenshinImpact.MonsterMap.Script;

/// <summary>
/// Online map detection class
/// </summary>
class DataInfo
{
    private const bool IsUseFakePicture = true;
    
    public static readonly Bitmap MainMap = (Bitmap)Image.FromFile("img/MainMap.jpg");
    public static Bitmap GameMap;
    public static PictureBox SampleImage; //Sample screenshots from the game
    public static PictureBox PointImage; //Feature point comparison screenshot

    public static IntPtr GenshinMainHandle
    {
        get
        {
            // HACK: TODO
            var process = GenshinProcess;
            
            const string key = "MAIN_HANDLE";
            if (process == null || process.HasExited)
            {
                MemoryCache.Default.Remove(key);
                return IntPtr.Zero;
            }
            
            var cacheHandle = MemoryCache.Default.Get(key);
            if (cacheHandle == null)
            {
                var handle = process.MainWindowHandle;
                if (handle == IntPtr.Zero)
                {
                    process.Refresh();
                    handle = process.MainWindowHandle;
                }

                if (handle == IntPtr.Zero)
                    return IntPtr.Zero;

                MemoryCache.Default.Add(key, handle, DateTimeOffset.MaxValue);
                return handle;
            }

            return (IntPtr)cacheHandle;
        }
    }
    
    public static int Width = 1920;
    public static int Height = 1080;
    public static bool IsDetection = false;
    public static bool IsShowLine = false;
    public static bool IsPauseShowIcon = false;
        
    private static Process[] GameProcess
    {
        get
        {
            if(IsUseFakePicture)
                return Process.GetProcessesByName("PhotosApp");
                
            return Process.GetProcessesByName("YuanShen")
                .Concat(Process.GetProcessesByName("GenshinImpact"))
                .ToArray();
        }
    }
    
    private static Process GenshinProcess => GameProcess.Any() ? GameProcess[0] : null;
}