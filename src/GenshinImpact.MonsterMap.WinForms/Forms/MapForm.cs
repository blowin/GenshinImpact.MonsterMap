using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinImpact.MonsterMap.Script;
using Timer = GenshinImpact.MonsterMap.Script.Timer;

namespace GenshinImpact.MonsterMap.Forms
{
    public partial class MapForm : Form
    {

        bool LastWindowIsYuanShen = false;
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
        public MapForm()
        {
            InitializeComponent();
            Graphics g = Graphics.FromImage(DataInfo.transparentMap);

            Task task = Task.Run(async () =>
            {
                while (!isJumpOutOfTask)
                {
                    if (DataInfo.isDetection)
                    {
                        DataInfo.isDetection = false;
                        try
                        {
                            Rectangle GameRect = new Rectangle();
                            Point gamePoint = new Point();
                            Win32Api.GetWindowRect(DataInfo.mainHandle, ref GameRect);
                            Win32Api.ClientToScreen(DataInfo.mainHandle, ref gamePoint);

                            Action changeSize = () => Size = new Size(DataInfo.width, DataInfo.height);
                            Invoke(changeSize);

                            Action changeLocation = () => Location = gamePoint;
                            Invoke(changeLocation);
                            DataInfo.gameMap = DataInfo.isUseFakePicture ? DataInfo.fakeMap : ImageUnitility.GetScreenshot(
                                DataInfo.mainHandle,
                                GameRect.Right - GameRect.Left,
                                GameRect.Bottom - GameRect.Top,
                                gamePoint.X - GameRect.Left,
                                gamePoint.Y - GameRect.Top
                                );
                            int scaleSrc = 1;
                            int scaleSub = 3;
                            Bitmap imgSrc = DataInfo.mainMap;
                            Bitmap imgSub = (Bitmap)DataInfo.gameMap.GetThumbnailImage(DataInfo.gameMap.Width / scaleSub, DataInfo.gameMap.Height / scaleSub, null, IntPtr.Zero);
                            var targetRect = ImageUnitility.MatchMap(imgSrc, imgSub, true, out Image outImage);
                            imgSub.Dispose();
                            var activePos = DataInfo.GetAllPos.Where(pos => DataInfo.selectTags.Contains(pos.Name)).ToList();
                            g.Clear(Color.Transparent);
                            if (!DataInfo.isPauseShowIcon)
                            {
                                activePos.ForEach(pos =>
                                {
                                    int x = (int)((pos.GetX(DataInfo.PixelPerIng, DataInfo.IngBias) - targetRect.X) * (Size.Width * 1.0f / targetRect.Width));
                                    int y = (int)((pos.GetY(DataInfo.PixelPerLat, DataInfo.LatBias) - targetRect.Y) * (Size.Height * 1.0f / targetRect.Height));
                                    Bitmap icon = DataInfo.iconDict[pos.Name];
                                    if ((x - icon.Width / 2) > 0 && (y - icon.Height) > 0)
                                    {
                                        if ((x - icon.Width / 2) < DataInfo.width && (y - icon.Height) < DataInfo.height)
                                        {
                                            g.DrawImage(DataInfo.iconDict[pos.Name], new PointF(x - icon.Width / 2, y - icon.Height));
                                        }
                                    }
                                });
                                if (DataInfo.isShowLine)
                                {
                                    for (int x = -100; x < 110; x += 10)
                                    {
                                        g.DrawLine(DataInfo.whitePen, x.ToMapPosX(targetRect, Size), -100.ToMapPosY(targetRect, Size), x.ToMapPosX(targetRect, Size), 100.ToMapPosY(targetRect, Size));
                                    }
                                    for (int y = -100; y < 110; y += 10)
                                    {
                                        g.DrawLine(DataInfo.whitePen, -100.ToMapPosX(targetRect, Size), y.ToMapPosY(targetRect, Size), 100.ToMapPosX(targetRect, Size), y.ToMapPosY(targetRect, Size));
                                    }
                                    g.DrawLine(DataInfo.redPen, -100.ToMapPosX(targetRect, Size), 0.ToMapPosY(targetRect, Size), 100.ToMapPosX(targetRect, Size), 0.ToMapPosY(targetRect, Size));
                                    g.DrawLine(DataInfo.redPen, 0.ToMapPosX(targetRect, Size), -100.ToMapPosY(targetRect, Size), 0.ToMapPosX(targetRect, Size), 100.ToMapPosY(targetRect, Size));
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
                    }
                    await Task.Delay(100);
                }
            });



        }
    }
}
