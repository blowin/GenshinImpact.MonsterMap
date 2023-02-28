using System.Drawing;
using OpenCvSharp;

namespace GenshinImpact.MonsterMap.Domain.ImageMatchers;

public class SURFImageMatcher : ImageMatcher
{
    public SURFImageMatcher(Bitmap imgSrc) : base(imgSrc)
    {
    }

    protected override Feature2D CreateFeature2D() 
        => OpenCvSharp.XFeatures2D.SURF.Create(400, 4, 3, true, true);

    protected override (List<Point2f> Src, List<Point2f> Dst, List<DMatch> Matches) GetMatchPoints(Mat matToRet, KeyPoint[] keyPointsTo)
    {
        var pointsSrc = new List<Point2f>();
        var pointsDst = new List<Point2f>();
        var goodMatches = new List<DMatch>();
        
        using var bfMatcher = new BFMatcher();
        var matches = bfMatcher.Match(MatSrcRet, matToRet);
        
        //Find the minimum and maximum distance
        double minDistance = 1000;//reverse approximation
        double maxDistance = 0;
        for (int i = 0; i < MatSrcRet.Rows; i++)
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
        for (int i = 0; i < MatSrcRet.Rows; i++)
        {
            double distance = matches[i].Distance;
            if (distance < Math.Max(minDistance * 2, 0.02))
            {
                pointsSrc.Add(KeyPointsSrc![matches[i].QueryIdx].Pt);
                pointsDst.Add(keyPointsTo[matches[i].TrainIdx].Pt);
                goodMatches.Add(matches[i]);
            }
        }

        return (pointsSrc, pointsDst, goodMatches);
    }
}