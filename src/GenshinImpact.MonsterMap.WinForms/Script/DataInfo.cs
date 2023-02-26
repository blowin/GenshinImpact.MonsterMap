using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.Domain;

namespace GenshinImpact.MonsterMap.Script;

/// <summary>
/// Online map detection class
/// </summary>
class DataInfo
{
    private static bool isUseFakePicture = true;
    
    public static Dictionary<string, Bitmap> iconDict = new();
    public static Bitmap mainMap = (Bitmap)Image.FromFile("img/MainMap.jpg");
    public static Bitmap transparentMap = (Bitmap)Image.FromFile("img/transparent.png");
    public static Bitmap fakeMap = (Bitmap)Image.FromFile("img/fake.jpg");
    public static Bitmap gameMap;
    public static Bitmap dealMap;
    public static PictureBox sampleImage; //Sample screenshots from the game
    public static PictureBox pointImage; //Feature point comparison screenshot
    public static Pen redPen = new Pen(new SolidBrush(Color.Red));
    public static Pen whitePen = new Pen(new SolidBrush(Color.White));
        
    public static Process GenshinProcess => gameProcess.Any() ? gameProcess[0] : null;

    public static IntPtr? mainHandle
    {
        get
        {
            // HACK: TODO
            const string key = "MAIN_HANDLE";
            if (GenshinProcess == null)
            {
                MemoryCache.Default.Remove(key);
                return null;
            }
            
            var cacheHandle = MemoryCache.Default.Get(key);
            if (cacheHandle == null)
            {
                var handle = GenshinProcess?.MainWindowHandle;
                if (handle == null || handle == IntPtr.Zero)
                    return handle;

                MemoryCache.Default.Add(key, handle.Value, DateTimeOffset.MaxValue);
                return handle;
            }

            return (IntPtr)cacheHandle;
        }
    }
    public static IntPtr hDeskTop = Win32Api.FindWindow("Progman ", "Program   Manager ");

    public static int width = 1920;
    public static int height = 1080;
    public static bool isDetection = false;
    public static bool isShowLine = false;
    public static bool isPauseShowIcon = false;
    public static bool isMapFormClose = false;
    public static List<string> selectTags = new List<string>();
        
    private static Process[] gameProcess
    {
        get
        {
            if(isUseFakePicture)
                return Process.GetProcessesByName("PhotosApp");
                
            return Process.GetProcessesByName("YuanShen")
                .Concat(Process.GetProcessesByName("GenshinImpact"))
                .ToArray();
        }
    }

    public static void LoadData()
    {
        foreach (var icon in new DirectoryInfo("icon").GetFiles())
        {
            iconDict[icon.Name] = (Bitmap)Image.FromFile(icon.FullName);
        }
    }
}