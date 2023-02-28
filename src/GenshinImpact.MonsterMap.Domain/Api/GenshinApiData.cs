using System.Globalization;
using GenshinImpact.MonsterMap.Domain.MapMarkers;

namespace GenshinImpact.MonsterMap.Domain.Api;

public class GenshinApiData
{
    public int code { get; set; }
    public string message { get; set; }
    public Datum[] data { get; set; }
    public class Datum
    {
        public int markType { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string imageLink { get; set; }
        public string wikiLink { get; set; }
        public string videoLink { get; set; }
        public string id { get; set; }
        public Point point { get; set; }
        public string icon { get; set; }
        public class Point
        {
            public string lng { get; set; }
            public string lat { get; set; }
        }

        public MapMarker ToFileIcon()
        {
            var lng = float.Parse(point.lng, CultureInfo.InvariantCulture.NumberFormat);
            var lat = float.Parse(point.lat, CultureInfo.InvariantCulture.NumberFormat);
            return new MapMarker(icon, lng, lat);
        }
    }
}