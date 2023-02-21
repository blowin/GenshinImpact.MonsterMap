using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;

namespace GenshinImpact.MonsterMap.Script
{
    /// <summary>
    /// Image matching and interception tools
    /// </summary>
    class ImageUnitility
    {
        /// <summary>
        /// Object Contrast Detection
        /// </summary>
        static Mat matSrcRet = new Mat();
        static KeyPoint[] keyPointsSrc = null;
        static KeyPoint[] keyPointsTo = null;
        public static Rectangle MatchMap(Bitmap imgSrc, Bitmap imgSub, bool useSift, out Image outImage)
        {
            Timer.Init();
            using (Mat matSrc = imgSrc.ToMat())
            using (Mat matTo = imgSub.ToMat())
            using (Mat matToRet = new Mat())
            {
                Console.WriteLine("////////////////////////////////////////");
                Timer.Show("start analysis");
                
                using (var useMatch = useSift ? (Feature2D)SIFT.Create() : (Feature2D)OpenCvSharp.XFeatures2D.SURF.Create(400, 4, 3, true, true))
                {
                    if (keyPointsSrc == null)
                    {
                        //Analyze points only on the first load of the big map
                        useMatch.DetectAndCompute(matSrc, null, out keyPointsSrc, matSrcRet);
                    }
                    Timer.Show("Extract large map feature points");
                    useMatch.DetectAndCompute(matTo, null, out keyPointsTo, matToRet);
                    Timer.Show("Extract game screenshot feature points");
                }
                using (var bfMatcher = new BFMatcher())
                {
                    var pointsSrc = new List<Point2f>();
                    var pointsDst = new List<Point2f>();
                    var goodMatches = new List<DMatch>();
                    if (useSift)
                    {
                        var matches = bfMatcher.KnnMatch(matSrcRet, matToRet, k: 2);
                        Timer.Show("Comparing feature points");
                        foreach (DMatch[] items in matches.Where(x => x.Length > 1))
                        {
                            if (items[0].Distance < 0.5 * items[1].Distance)
                            {
                                pointsSrc.Add(keyPointsSrc[items[0].QueryIdx].Pt);
                                pointsDst.Add(keyPointsTo[items[0].TrainIdx].Pt);
                                goodMatches.Add(items[0]);
                            }
                        }
                    }
                    else
                    {
                        var matches = bfMatcher.Match(matSrcRet, matToRet);
                        Timer.Show("Comparing feature points");
                        //Find the minimum and maximum distance
                        double minDistance = 1000;//reverse approximation
                        double maxDistance = 0;
                        for (int i = 0; i < matSrcRet.Rows; i++)
                        {
                            double distance = matches[i].Distance;
                            if (distance > maxDistance)
                            {
                                maxDistance = distance;
                            }
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                            }
                        }
                        for (int i = 0; i < matSrcRet.Rows; i++)
                        {
                            double distance = matches[i].Distance;
                            if (distance < Math.Max(minDistance * 2, 0.02))
                            {
                                pointsSrc.Add(keyPointsSrc[matches[i].QueryIdx].Pt);
                                pointsDst.Add(keyPointsTo[matches[i].TrainIdx].Pt);
                                goodMatches.Add(matches[i]);
                            }
                        }
                    }
                    using (var outMat = new Mat())
                    using (var outMask = new Mat())
                    {
                        var pSrc = pointsSrc.ConvertAll(point => new Point2d((int)point.X, (int)point.Y));
                        var pDst = pointsDst.ConvertAll(point => new Point2d((int)point.X, (int)point.Y));
                        // If the original matching result is empty, the filtering step is skipped
                        if (pSrc.Count > 4 && pDst.Count > 4)
                            Cv2.FindHomography(pSrc, pDst, HomographyMethods.Ransac, mask: outMask);
                        Timer.Show("Filtered");

                        /*
                         If the number of matching points processed by RANSAC is greater than 10, the filter is applied. Otherwise, 
                         the original matching point result is used (when there are too few matching points processed by RANSAC, 
                         the result of 0 matching points may be obtained).
                         */
                        if (outMask.Rows > 10)
                        {
                            byte[] maskBytes = new byte[outMask.Rows * outMask.Cols];
                            outMask.GetArray(out maskBytes);
                            for (int i = maskBytes.Count() - 1; i < 0; i--)
                            {
                                if (maskBytes[i] == 1)
                                {
                                    pointsSrc.RemoveAt(i);
                                    pointsDst.RemoveAt(i);
                                }

                            }
                            Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, matchesMask: maskBytes, flags: DrawMatchesFlags.NotDrawSinglePoints);
                        }
                        else
                        {
                            Cv2.DrawMatches(matSrc, keyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, flags: DrawMatchesFlags.NotDrawSinglePoints);
                        }
                        if (DataInfo.dealMap != null) DataInfo.dealMap.Dispose();
                        DataInfo.dealMap = BitmapConverter.ToBitmap(outMat);
                        outImage = DataInfo.dealMap;
                        var pointOriginX_R = pointsSrc.OrderBy(point => point.X).FirstOrDefault();//The rightmost point of the original image
                        var pointOriginX_L = pointsSrc.OrderByDescending(point => point.X).FirstOrDefault();//The leftmost point of the original image

                        var pointTargetX_R = pointsDst.OrderBy(point => point.X).FirstOrDefault();//The point on the far right of the test graph
                        var pointTargetX_L = pointsDst.OrderByDescending(point => point.X).FirstOrDefault();//The leftmost point of the test graph

                        float scaleX = (pointOriginX_R.X - pointOriginX_L.X) / (pointTargetX_R.X - pointTargetX_L.X);
                        float scaleY = (pointOriginX_R.Y - pointOriginX_L.Y) / (pointTargetX_R.Y - pointTargetX_L.Y);

                        var targetWidth = imgSub.Width;
                        var targetHeigh = imgSub.Height;

                        float rectBaisXL = pointOriginX_R.X - pointTargetX_R.X * scaleX;
                        float rectBaisXR = pointOriginX_L.X + (targetWidth - pointTargetX_L.X) * scaleX;

                        float rectBaisYU = pointOriginX_R.Y - pointTargetX_R.Y * scaleY;
                        float rectBaisYD = pointOriginX_L.Y + (targetHeigh - pointTargetX_L.Y) * scaleY;
                        Timer.Show("变换坐标系");
                        return new Rectangle((int)rectBaisXL, (int)rectBaisYU, (int)(rectBaisXR - rectBaisXL), (int)(rectBaisYD - rectBaisYU));
                    }
                    // Algorithm RANSAC filters the matching results

                }
            }
        }
        public static Bitmap GetScreenshot(IntPtr hWnd, int w, int h, int x, int y)
        {
            IntPtr hscrdc = Win32Api.GetWindowDC(hWnd);
            IntPtr hbitmap = Win32Api.CreateCompatibleBitmap(hscrdc, DataInfo.width + x, DataInfo.height + y);
            IntPtr hmemdc = Win32Api.CreateCompatibleDC(hscrdc);
            Win32Api.SelectObject(hmemdc, hbitmap);
            Win32Api.PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Image.FromHbitmap(hbitmap);
            Win32Api.DeleteDC(hscrdc);
            Win32Api.DeleteDC(hmemdc);
            if (DataInfo.width>0&& DataInfo.height>0)
            {
                Bitmap clipBitamp = new Bitmap(DataInfo.width, DataInfo.height);
                using (Graphics g = Graphics.FromImage(clipBitamp))
                {
                    g.DrawImage(
                        bmp,
                        new Rectangle(0, 0, DataInfo.width, DataInfo.height),
                        new Rectangle(x, y, DataInfo.width, DataInfo.height),
                        GraphicsUnit.Pixel);
                }
                bmp.Dispose();
                return clipBitamp;
            }
            else
            {
                return new Bitmap(1, 1);
            }
           
        }
    }
}
