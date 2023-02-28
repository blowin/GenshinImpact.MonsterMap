using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.Domain;
using GenshinImpact.MonsterMap.Script;
using Icon = GenshinImpact.MonsterMap.Domain.Icons.Icon;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = GenshinImpact.MonsterMap.Script.Timer;

namespace GenshinImpact.MonsterMap.Forms;

[SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
public partial class MapForm : Form
{
    private static readonly Pen RedPen = new Pen(new SolidBrush(Color.Red));
    private static readonly Pen WhitePen = new Pen(new SolidBrush(Color.White));
    private static readonly Dictionary<string, Bitmap> IconDict = LoadData();
    private static readonly Bitmap TransparentMap = (Bitmap)Image.FromFile("img/transparent.png");
    private static readonly IntPtr HDeskTop = Win32Api.FindWindow("Progman ", "Program   Manager ");
    
    private readonly FileSystemBias _bias;
    private readonly System.Timers.Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private Bitmap _dealMap;
    private bool _lastWindowIsGenshin;
    private bool _isJumpOutOfTask;
    
    private bool IsTopOfGenshin => Win32Api.GetForegroundWindow() == DataInfo.GenshinMainHandle; //The current top is Genshin

    public MapForm(FileSystemBias bias, Func<IEnumerable<Icon>> iconLoader)
    {
        _bias = bias;
        _cancellationTokenSource = new CancellationTokenSource();
        InitializeComponent();
        
        Closing += OnClosing;
        
        Graphics g = Graphics.FromImage(TransparentMap);
        var _ = Task.Run(async () => await RunMapJob(g, iconLoader, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
        _timer = new System.Timers.Timer(TimeSpan.FromMilliseconds(100));
        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
    }
    
    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        if (!IsTopOfGenshin)
        {
            _lastWindowIsGenshin = false;
            return;
        }

        if (!_lastWindowIsGenshin) //The original god process was not on the top when it was detected last time
        {
            Console.WriteLine("Re-top");
            Win32Api.SetParent(Handle, HDeskTop); //top
        }

        _lastWindowIsGenshin = true;
    }

    private async Task RunMapJob(Graphics g, Func<IEnumerable<Icon>> iconLoader, CancellationToken cancellationToken)
    {
        while (!_isJumpOutOfTask && !cancellationToken.IsCancellationRequested)
        {
            var handle = DataInfo.GenshinMainHandle;
            if (!DataInfo.IsDetection || handle == IntPtr.Zero)
            {
                await Task.Delay(100, cancellationToken);
                continue;
            }

            DataInfo.IsDetection = false;
            try
            {
                var gamePoint = new Point();
                Win32Api.ClientToScreen(handle, ref gamePoint);

                Action changeSize = () => Size = new Size(DataInfo.Width, DataInfo.Height);
                Invoke(changeSize);

                Action changeLocation = () => Location = gamePoint;
                Invoke(changeLocation);
                
                var currentGameMap = ImageUnitility.GetScreenshot(handle, gamePoint);
                if (currentGameMap == null)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }
                
                DataInfo.GameMap = currentGameMap; 
                
                int scaleSrc = 1;
                int scaleSub = 3;
                Bitmap imgSrc = DataInfo.MainMap;
                Bitmap imgSub = (Bitmap)DataInfo.GameMap.GetThumbnailImage(DataInfo.GameMap.Width / scaleSub,
                    DataInfo.GameMap.Height / scaleSub, null, IntPtr.Zero);
                Rectangle targetRect = ImageUnitility.MatchMap(imgSrc, imgSub, true, out var outImage);
                _dealMap?.Dispose();
                _dealMap = outImage;
                imgSub.Dispose();

                g.Clear(Color.Transparent);
                if (!DataInfo.IsPauseShowIcon)
                {
                    foreach (var pos in iconLoader())
                    {
                        int x = (int)((pos.GetX(_bias.PixelPerIng, _bias.IngBias) - targetRect.X) *
                                      (Size.Width * 1.0f / targetRect.Width));
                        int y = (int)((pos.GetY(_bias.PixelPerLat, _bias.LatBias) - targetRect.Y) *
                                      (Size.Height * 1.0f / targetRect.Height));
                        Bitmap icon = IconDict[pos.Name];
                        if ((x - icon.Width / 2) > 0 && (y - icon.Height) > 0)
                        {
                            if ((x - icon.Width / 2) < DataInfo.Width && (y - icon.Height) < DataInfo.Height)
                            {
                                g.DrawImage(icon, new PointF(x - icon.Width / 2, y - icon.Height));
                            }
                        }
                    }
               
                    if (DataInfo.IsShowLine)
                    {
                        for (int x = -100; x < 110; x += 10)
                        {
                            g.DrawLine(WhitePen, _bias.ToMapPosX(x, targetRect, Size),
                                _bias.ToMapPosY(-100, targetRect, Size), _bias.ToMapPosX(x, targetRect, Size),
                                _bias.ToMapPosY(100, targetRect, Size));
                        }

                        for (int y = -100; y < 110; y += 10)
                        {
                            g.DrawLine(WhitePen, _bias.ToMapPosX(-100, targetRect, Size),
                                _bias.ToMapPosY(y, targetRect, Size), _bias.ToMapPosX(100, targetRect, Size),
                                _bias.ToMapPosY(y, targetRect, Size));
                        }

                        g.DrawLine(RedPen, _bias.ToMapPosX(-100, targetRect, Size),
                            _bias.ToMapPosY(0, targetRect, Size), _bias.ToMapPosX(100, targetRect, Size),
                            _bias.ToMapPosY(0, targetRect, Size));
                        g.DrawLine(RedPen, _bias.ToMapPosX(0, targetRect, Size),
                            _bias.ToMapPosY(-100, targetRect, Size), _bias.ToMapPosX(0, targetRect, Size),
                            _bias.ToMapPosY(100, targetRect, Size));
                    }
                }


                Console.WriteLine("Coordinates drawn");
                DataInfo.SampleImage.Image = DataInfo.GameMap;
                DataInfo.PointImage.Image = _dealMap;
                Console.WriteLine(DataInfo.GameMap.Size);
                if (!_isJumpOutOfTask)
                {
                    Action refreshImage = () => pictureBox1.Image = TransparentMap;
                    Invoke(refreshImage);
                }

                Timer.Show("The picture is updated");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            await Task.Delay(100, cancellationToken);
        }
    }
    
    private void OnClosing(object sender, CancelEventArgs e)
    {
        _isJumpOutOfTask = true;
        _timer.Stop();
        _timer.Dispose();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
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