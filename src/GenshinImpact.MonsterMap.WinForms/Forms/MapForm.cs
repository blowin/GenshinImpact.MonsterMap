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
using GenshinImpact.MonsterMap.Domain.Icons;
using GenshinImpact.MonsterMap.Script;
using Icon = GenshinImpact.MonsterMap.Domain.Icons.Icon;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = GenshinImpact.MonsterMap.Script.Timer;

namespace GenshinImpact.MonsterMap.Forms;

[SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
public partial class MapForm : Form
{
    private readonly FileSystemBias _bias;
    private readonly System.Timers.Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _job;
    
    private bool _lastWindowIsYuanShen;
    private bool _isJumpOutOfTask;
    
    public MapForm(FileSystemBias bias, Func<IEnumerable<Icon>> iconLoader)
    {
        _bias = bias;
        _cancellationTokenSource = new CancellationTokenSource();
        InitializeComponent();
        
        Closing += OnClosing;
        
        Graphics g = Graphics.FromImage(DataInfo.transparentMap);
        _job = Task.Run(async () => await RunMapJob(g, iconLoader, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
        _timer = new System.Timers.Timer(TimeSpan.FromMilliseconds(100));
        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
    }
    
    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        IntPtr ForegrouindWindow = Win32Api.GetForegroundWindow();
        if (ForegrouindWindow == DataInfo.mainHandle)//The current top is Genshin
        {
            if (!_lastWindowIsYuanShen)//The original god process was not on the top when it was detected last time
            {
                Console.WriteLine("######################################################");
                Console.WriteLine("Re-top");
                Win32Api.SetParent(Handle, DataInfo.hDeskTop);//top
                Console.WriteLine("######################################################");
            }
            _lastWindowIsYuanShen = true;
        }
        else
        {
            _lastWindowIsYuanShen = false;
        }
    }

    private async Task RunMapJob(Graphics g, Func<IEnumerable<Icon>> iconLoader, CancellationToken cancellationToken)
    {
        while (!_isJumpOutOfTask && !cancellationToken.IsCancellationRequested)
        {
            var handle = DataInfo.mainHandle;
            if (!DataInfo.isDetection || handle == IntPtr.Zero)
            {
                await Task.Delay(100, cancellationToken);
                continue;
            }

            DataInfo.isDetection = false;
            try
            {
                var gamePoint = new Point();
                Win32Api.ClientToScreen(handle, ref gamePoint);

                Action changeSize = () => Size = new Size(DataInfo.width, DataInfo.height);
                Invoke(changeSize);

                Action changeLocation = () => Location = gamePoint;
                Invoke(changeLocation);
                
                var currentGameMap = ImageUnitility.GetScreenshot(handle, gamePoint);
                if (currentGameMap == null)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }
                
                DataInfo.gameMap = currentGameMap; 
                
                int scaleSrc = 1;
                int scaleSub = 3;
                Bitmap imgSrc = DataInfo.mainMap;
                Bitmap imgSub = (Bitmap)DataInfo.gameMap.GetThumbnailImage(DataInfo.gameMap.Width / scaleSub,
                    DataInfo.gameMap.Height / scaleSub, null, IntPtr.Zero);
                var targetRect = ImageUnitility.MatchMap(imgSrc, imgSub, true, out Image outImage);
                imgSub.Dispose();

                g.Clear(Color.Transparent);
                if (!DataInfo.isPauseShowIcon)
                {
                    foreach (var pos in iconLoader())
                    {
                        int x = (int)((pos.GetX(_bias.PixelPerIng, _bias.IngBias) - targetRect.X) *
                                      (Size.Width * 1.0f / targetRect.Width));
                        int y = (int)((pos.GetY(_bias.PixelPerLat, _bias.LatBias) - targetRect.Y) *
                                      (Size.Height * 1.0f / targetRect.Height));
                        Bitmap icon = DataInfo.iconDict[pos.Name];
                        if ((x - icon.Width / 2) > 0 && (y - icon.Height) > 0)
                        {
                            if ((x - icon.Width / 2) < DataInfo.width && (y - icon.Height) < DataInfo.height)
                            {
                                g.DrawImage(DataInfo.iconDict[pos.Name],
                                    new PointF(x - icon.Width / 2, y - icon.Height));
                            }
                        }
                    }
               
                    if (DataInfo.isShowLine)
                    {
                        for (int x = -100; x < 110; x += 10)
                        {
                            g.DrawLine(DataInfo.whitePen, _bias.ToMapPosX(x, targetRect, Size),
                                _bias.ToMapPosY(-100, targetRect, Size), _bias.ToMapPosX(x, targetRect, Size),
                                _bias.ToMapPosY(100, targetRect, Size));
                        }

                        for (int y = -100; y < 110; y += 10)
                        {
                            g.DrawLine(DataInfo.whitePen, _bias.ToMapPosX(-100, targetRect, Size),
                                _bias.ToMapPosY(y, targetRect, Size), _bias.ToMapPosX(100, targetRect, Size),
                                _bias.ToMapPosY(y, targetRect, Size));
                        }

                        g.DrawLine(DataInfo.redPen, _bias.ToMapPosX(-100, targetRect, Size),
                            _bias.ToMapPosY(0, targetRect, Size), _bias.ToMapPosX(100, targetRect, Size),
                            _bias.ToMapPosY(0, targetRect, Size));
                        g.DrawLine(DataInfo.redPen, _bias.ToMapPosX(0, targetRect, Size),
                            _bias.ToMapPosY(-100, targetRect, Size), _bias.ToMapPosX(0, targetRect, Size),
                            _bias.ToMapPosY(100, targetRect, Size));
                    }
                }


                Console.WriteLine("Coordinates drawn");
                DataInfo.sampleImage.Image = DataInfo.gameMap;
                DataInfo.pointImage.Image = DataInfo.dealMap;
                Console.WriteLine(DataInfo.gameMap.Size);
                if (!_isJumpOutOfTask)
                {
                    Action refreshImage = () => pictureBox1.Image = DataInfo.transparentMap;
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
        _timer.Dispose();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}