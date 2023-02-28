using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Features2D;

namespace GenshinImpact.MonsterMap.Domain.ImageMatchers;

public sealed class SIFTImageMatcher : ImageMatcher
{
    public SIFTImageMatcher(Bitmap imgSrc) : base(imgSrc)
    {
    }

    protected override Feature2D CreateFeature2D() => SIFT.Create();

    protected override (List<Point2f> Src, List<Point2f> Dst, List<DMatch> Matches) GetMatchPoints(Mat matToRet, KeyPoint[] keyPointsTo)
    {
        var pointsSrc = new List<Point2f>();
        var pointsDst = new List<Point2f>();
        var goodMatches = new List<DMatch>();
        
        using var bfMatcher = new BFMatcher();
        var matches = bfMatcher.KnnMatch(MatSrcRet, matToRet, k: 2);
        foreach (var items in matches.Where(x => x.Length > 1))
        {
            if (items[0].Distance < 0.5 * items[1].Distance)
            {
                pointsSrc.Add(KeyPointsSrc![items[0].QueryIdx].Pt);
                pointsDst.Add(keyPointsTo[items[0].TrainIdx].Pt);
                goodMatches.Add(items[0]);
            }
        }

        return (pointsSrc, pointsDst, goodMatches);
    }
}