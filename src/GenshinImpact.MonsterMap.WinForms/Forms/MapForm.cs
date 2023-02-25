using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.Domain;
using GenshinImpact.MonsterMap.Script;
using Timer = GenshinImpact.MonsterMap.Script.Timer;

namespace GenshinImpact.MonsterMap.Forms
{
    [SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
    public partial class MapForm : Form
    {
        private FileSystemBias _bias;
        private bool LastWindowIsYuanShen = false;
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            IntPtr ForegrouindWindow = Win32Api.GetForegroundWindow();
            if (ForegrouindWindow == DataInfo.mainHandle)//The current top is Genshin
            {
                if (!LastWindowIsYuanShen)//The original god process was not on the top when it was detected last time
                {
                    Console.WriteLine("######################################################");
                    Console.WriteLine("Re-top");
                    Win32Api.SetParent(Handle, DataInfo.hDeskTop);//top
                    Console.WriteLine("######################################################");
                }
                LastWindowIsYuanShen = true;
            }
            else
            {
                LastWindowIsYuanShen = false;
            }
        }
        public bool isJumpOutOfTask = false;
        public MapForm(FileSystemBias bias, CancellationToken cancellationToken)
        {
            _bias = bias;
            InitializeComponent();
            Graphics g = Graphics.FromImage(DataInfo.transparentMap);

            Task.Run(async () => await RunMapJob(g, cancellationToken), cancellationToken);
        }

        private async Task RunMapJob(Graphics g, CancellationToken cancellationToken)
        {
            while (!isJumpOutOfTask && !cancellationToken.IsCancellationRequested)
            {
                if (!DataInfo.isDetection)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                DataInfo.isDetection = false;
                try
                {
                    Rectangle gameRect = new Rectangle();
                    Point gamePoint = new Point();
                    Win32Api.GetWindowRect(DataInfo.mainHandle, ref gameRect);
                    Win32Api.ClientToScreen(DataInfo.mainHandle, ref gamePoint);

                    Action changeSize = () => Size = new Size(DataInfo.width, DataInfo.height);
                    Invoke(changeSize);

                    Action changeLocation = () => Location = gamePoint;
                    Invoke(changeLocation);
                    DataInfo.gameMap = DataInfo.isUseFakePicture
                        ? DataInfo.fakeMap
                        : ImageUnitility.GetScreenshot(
                            DataInfo.mainHandle,
                            gameRect.Right - gameRect.Left,
                            gameRect.Bottom - gameRect.Top,
                            gamePoint.X - gameRect.Left,
                            gamePoint.Y - gameRect.Top
                        );
                    int scaleSrc = 1;
                    int scaleSub = 3;
                    Bitmap imgSrc = DataInfo.mainMap;
                    Bitmap imgSub = (Bitmap)DataInfo.gameMap.GetThumbnailImage(DataInfo.gameMap.Width / scaleSub,
                        DataInfo.gameMap.Height / scaleSub, null, IntPtr.Zero);
                    var targetRect = ImageUnitility.MatchMap(imgSrc, imgSub, true, out Image outImage);
                    imgSub.Dispose();
                    var activePos = DataInfo.GetAllPos.Where(pos => DataInfo.selectTags.Contains(pos.Name))
                        .ToList();
                    g.Clear(Color.Transparent);
                    if (!DataInfo.isPauseShowIcon)
                    {
                        activePos.ForEach(pos =>
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
                        });
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
                    if (!isJumpOutOfTask)
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
    }
}
