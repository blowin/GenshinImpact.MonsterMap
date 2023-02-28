using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GenshinImpact.MonsterMap.Domain;
using Point = System.Drawing.Point;

namespace GenshinImpact.MonsterMap.Script;

/// <summary>
/// Image matching and interception tools
/// </summary>
class ImageUnitility
{
    public static Bitmap? GetScreenshot(IntPtr hWnd, Point point)
    {
        if (hWnd == IntPtr.Zero)
            return null;
        
        var rectangle = new RECT();
        Win32Api.GetWindowRect(hWnd, ref rectangle);
        
        int width = rectangle.Right - rectangle.Left;
        int height = rectangle.Bottom - rectangle.Top;

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        var bmp = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bmp);
        graphics.DrawImage(
            bmp,
            new Rectangle(0, 0, width, height),
            new Rectangle(point.X, point.Y, width, height),
            GraphicsUnit.Pixel);
        graphics.CopyFromScreen(new Point(rectangle.Left, rectangle.Top), Point.Empty, bmp.Size);
        return bmp;  
    }
}