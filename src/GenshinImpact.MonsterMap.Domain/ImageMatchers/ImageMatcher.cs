using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Features2D;

namespace GenshinImpact.MonsterMap.Domain.ImageMatchers;

/// <summary>
/// Object Contrast Detection
/// </summary>
public abstract class ImageMatcher
{
    private readonly Bitmap _imgSrc;
    protected readonly Mat MatSrcRet = new();
    protected KeyPoint[]? KeyPointsSrc;

    public ImageMatcher(Bitmap imgSrc)
    {
        _imgSrc = imgSrc;
    }
    
    public Rectangle MatchMap(Bitmap imgSub, out Bitmap outImage)
    {
        using var matSrc = _imgSrc.ToMat();
        using var matTo = imgSub.ToMat();
        using var matToRet = new Mat();

        var keyPointsTo = CreateKeyPointsTo(matSrc, matTo, matToRet);
        var (pointsSrc, pointsDst, goodMatches) = GetMatchPoints(matToRet, keyPointsTo);

        using var outMat = new Mat();
        using var outMask = new Mat();
        
        var pSrc = pointsSrc.ConvertAll(point => new Point2d((int)point.X, (int)point.Y));
        var pDst = pointsDst.ConvertAll(point => new Point2d((int)point.X, (int)point.Y));
        // If the original matching result is empty, the filtering step is skipped
        if (pSrc.Count > 4 && pDst.Count > 4)
            Cv2.FindHomography(pSrc, pDst, HomographyMethods.Ransac, mask: outMask);
            
        DrawMatches(matSrc, pointsSrc, pointsDst, matTo, keyPointsTo, outMat, outMask, goodMatches);
        outImage = outMat.ToBitmap();

        var (pointOriginX_R, pointOriginX_L) = GetMostPointOfImage(pointsSrc);
        var (pointTargetX_R, pointTargetX_L) = GetMostPointOfImage(pointsDst);
        
        var scaleX = (pointOriginX_R.X - pointOriginX_L.X) / (pointTargetX_R.X - pointTargetX_L.X);
        var scaleY = (pointOriginX_R.Y - pointOriginX_L.Y) / (pointTargetX_R.Y - pointTargetX_L.Y);

        var targetWidth = imgSub.Width;
        var targetHeigh = imgSub.Height;

        var rectBaisXL = pointOriginX_R.X - pointTargetX_R.X * scaleX;
        var rectBaisXR = pointOriginX_L.X + (targetWidth - pointTargetX_L.X) * scaleX;

        var rectBaisYU = pointOriginX_R.Y - pointTargetX_R.Y * scaleY;
        var rectBaisYD = pointOriginX_L.Y + (targetHeigh - pointTargetX_L.Y) * scaleY;
        return new Rectangle((int)rectBaisXL, (int)rectBaisYU, (int)(rectBaisXR - rectBaisXL), (int)(rectBaisYD - rectBaisYU));
    }

    private KeyPoint[] CreateKeyPointsTo(Mat matSrc, Mat matTo, Mat matToRet)
    {
        using var useMatch = CreateFeature2D();
        if (KeyPointsSrc == null)
        {
            //Analyze points only on the first load of the big map
            useMatch.DetectAndCompute(matSrc, null, out KeyPointsSrc, MatSrcRet);
        }

        useMatch.DetectAndCompute(matTo, null, out var keyPointsTo, matToRet);
        return keyPointsTo;
    }

    /// <summary>
    /// If the number of matching points processed by RANSAC is greater than 10, the filter is applied. Otherwise, 
    /// the original matching point result is used (when there are too few matching points processed by RANSAC, 
    /// the result of 0 matching points may be obtained).
    /// </summary>
    /// <param name="matSrc"></param>
    /// <param name="pointsSrc"></param>
    /// <param name="pointsDst"></param>
    /// <param name="matTo"></param>
    /// <param name="keyPointsTo"></param>
    /// <param name="outMat"></param>
    /// <param name="outMask"></param>
    /// <param name="goodMatches"></param>
    private void DrawMatches(Mat matSrc, List<Point2f> pointsSrc, List<Point2f> pointsDst, 
        Mat matTo, KeyPoint[] keyPointsTo, Mat outMat, Mat outMask, List<DMatch> goodMatches)
    {
        if (outMask.Rows > 10)
        {
            outMask.GetArray(out byte[] maskBytes);
            for (var i = maskBytes.Length - 1; i < 0; i--)
            {
                if (maskBytes[i] != 1) 
                    continue;
                
                pointsSrc.RemoveAt(i);
                pointsDst.RemoveAt(i);
            }

            Cv2.DrawMatches(matSrc, KeyPointsSrc, matTo, keyPointsTo, goodMatches, outMat, matchesMask: maskBytes,
                flags: DrawMatchesFlags.NotDrawSinglePoints);
        }
        else
        {
            Cv2.DrawMatches(matSrc, KeyPointsSrc, matTo, keyPointsTo, goodMatches, outMat,
                flags: DrawMatchesFlags.NotDrawSinglePoints);
        }
    }

    private static (Point2f R, Point2f L) GetMostPointOfImage(List<Point2f>? points)
    {
        if (points == null || points.Count == 0)
            return (new Point2f(), new Point2f());
        
        var pointR = points.MinBy(e => e.X);//The rightmost point of the original image
        var pointL = points.MaxBy(e => e.X);//The leftmost point of the original image
        return (pointR, pointL);
    }
    
    protected abstract Feature2D CreateFeature2D();
    
    protected abstract (List<Point2f> Src, List<Point2f> Dst, List<DMatch> Matches) GetMatchPoints(Mat matToRet, KeyPoint[] keyPointsTo);
}