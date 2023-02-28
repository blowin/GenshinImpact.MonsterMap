using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.Domain;
using GenshinImpact.MonsterMap.Domain.GameProcesses.GameProcessProviders;
using GenshinImpact.MonsterMap.Domain.ImageMatchers;
using GenshinImpact.MonsterMap.Script;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = GenshinImpact.MonsterMap.Script.Timer;

namespace GenshinImpact.MonsterMap.Forms;

[SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
public partial class MapForm : Form
{
    private static readonly Bitmap TransparentMap = (Bitmap)Image.FromFile("img/transparent.png");
    private static readonly IntPtr HDeskTop = Win32Api.FindWindow("Progman ", "Program   Manager ");
    
    private readonly System.Timers.Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MapInfoDrawer _drawer;
    private readonly IGameProcessProvider _gameProcessProvider;
    private readonly ImageMatcher _imageMatcher;
    private readonly GameSize _gameSize;

    private Bitmap _dealMap;
    private bool _lastWindowIsGenshin;
    private bool _isJumpOutOfTask;
    
    private static TimeSpan DelayTime => TimeSpan.FromMilliseconds(100);
    
    public MapForm(MapInfoDrawer drawer, IGameProcessProvider gameProcessProvider, ImageMatcher imageMatcher, GameSize gameSize)
    {
        _drawer = drawer;
        _gameProcessProvider = gameProcessProvider;
        _imageMatcher = imageMatcher;
        _gameSize = gameSize;
        _cancellationTokenSource = new CancellationTokenSource();
        InitializeComponent();
        
        Closing += OnClosing;
        var _ = Task.Run(async () => await RunMapJob(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        _timer = new System.Timers.Timer(TimeSpan.FromMilliseconds(100));
        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
    }

    private async Task RunMapJob(CancellationToken cancellationToken)
    {
        var graphics = Graphics.FromImage(TransparentMap);
        
        while (!_isJumpOutOfTask && !cancellationToken.IsCancellationRequested)
        {
            var process = _gameProcessProvider.GetProcess();
            var handle = process.MainWindowHandle;
            if (!DataInfo.IsDetection || handle == IntPtr.Zero || process.HasExited)
            {
                await Task.Delay(DelayTime, cancellationToken);
                continue;
            }

            DataInfo.IsDetection = false;
            try
            {
                var gamePoint = new Point();
                Win32Api.ClientToScreen(handle, ref gamePoint);

                Action changeSize = () => Size = new Size(_gameSize.Width, _gameSize.Height);
                Invoke(changeSize);

                Action changeLocation = () => Location = gamePoint;
                Invoke(changeLocation);
                
                var currentGameMap = ImageUnitility.GetScreenshot(handle, gamePoint);
                if (currentGameMap == null)
                {
                    await Task.Delay(DelayTime, cancellationToken);
                    continue;
                }
                
                DataInfo.GameMap = currentGameMap; 
                
                var targetRect = GetTargetRect(DataInfo.GameMap);
                graphics.Clear(Color.Transparent);
                if (targetRect.Height <= 0 || targetRect.Width <= 0)
                {
                    await Task.Delay(DelayTime, cancellationToken);
                    continue;
                }
                
                if (!DataInfo.IsPauseShowIcon)
                {
                    _drawer.DrawMarkers(graphics, targetRect);

                    if (DataInfo.IsShowLine)
                        _drawer.DrawLines(graphics, targetRect);
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

            await Task.Delay(DelayTime, cancellationToken);
        }
    }

    private Rectangle GetTargetRect(Bitmap gameMap)
    {
        const int scaleSub = 3;
        var thumbWidth = gameMap.Width / scaleSub;
        var thumbHeight = gameMap.Height / scaleSub;
        using var imgSub = (Bitmap)gameMap.GetThumbnailImage(thumbWidth, thumbHeight, null, IntPtr.Zero);
        var targetRect = _imageMatcher.MatchMap(imgSub, out var outImage);
        _dealMap?.Dispose();
        _dealMap = outImage;
        imgSub.Dispose();
        return targetRect;
    }
    
    private void OnClosing(object sender, CancelEventArgs e)
    {
        _isJumpOutOfTask = true;
        _timer.Stop();
        _timer.Dispose();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
    
    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        var process = _gameProcessProvider.GetProcess();
        if (!process.IsTopOfProcess)
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
}