using AngleSharp.Dom;
using AngleSharp.Html.Dom;

var path = "D:\\222";
Combine(new List<Image<Rgba32>>
{
    Image.Load<Rgba32>("D:\\222\\1.jpg"),
    Image.Load<Rgba32>("D:\\222\\9b07c634-8e26-11ed-843b-ac1f6b44f230.jpg"),
    Image.Load<Rgba32>("D:\\222\\99cef8dc-8e26-11ed-b9ca-ac1f6b44f230.jpg"),
    Image.Load<Rgba32>("D:\\222\\956c8b4c-8e26-11ed-abf9-ac1f6b44f230.jpg"),
}, 2, 2);

static Image<Rgba32> CombineFile(string path, int columnCount, int rowCount)
{
    var files = Directory.GetFiles(path);

    var images = files.Select(Image.Load<Rgba32>).ToList();
    try
    {
        return Combine(images, columnCount, rowCount);
    }
    finally
    {
        foreach (var image in images)
            image.Dispose();
    }
}

static Image<Rgba32> Combine(List<Image<Rgba32>> images, int columnCount, int rowCount)
{
    var imgSize = images.First().Size;
    var result = new Image<Rgba32>(imgSize.Width * columnCount, imgSize.Height * rowCount);
    result.Mutate(o =>
    {
        var column = 0;
        var row = 0;
        for (var i = 0; i < images.Count; i++)
        {
            var img = images[i];
            o.DrawImage(img, new Point(column * imgSize.Width, row * imgSize.Height), 1f);
            column += 1;
                
            if ((i + 1) % rowCount == 0)
            {
                row += 1;
                column = 0;
            }
        }
    });
    result.Save("D:\\huge_img.png");
    return result;
}

/*
foreach (var file in Directory.GetFiles("Locations"))
{
    var config = Configuration.Default.WithDefaultLoader();
    var context = BrowsingContext.New(config);
    await using var htmlFile = File.OpenRead(file);
    var document = await context.OpenAsync(req => req.Content(htmlFile));
    
    var markers = document.QuerySelectorAll("label")
        .Where(e => !string.IsNullOrEmpty(e.Id) && e.Id.StartsWith("node-"))
        .OfType<IHtmlLabelElement>()
        .ToList();
    
    var filters = document.All.First(el => el.LocalName == "div" && el.Id == "worldmap-filters");
    
    var mappedFilters = filters.Children.Select(ToMarkerGroup).ToList();

    var parsedFilters = mappedFilters.SelectMany(e => e.Markers)
        .Select(e => e.FilterName)
        .ToHashSet(StringComparer.InvariantCultureIgnoreCase);
    
    var filteredMarkers = markers
        .Where(e => e.Attributes.Any(atr => atr.NodeName == "data-filter" && parsedFilters.Contains(atr.Value)))
        .ToList();
    
    var s = 1;
}
*/
static MarkerGroup ToMarkerGroup(IElement element)
{
    var name = element.QuerySelectorAll("h2").First().Attributes["data-section-title"].Value;
    var markers = element.QuerySelectorAll("label").Select(ToMarker).ToArray();
    return new MarkerGroup(name, markers);
}

static Marker ToMarker(IElement element)
{
    var name = element.Attributes["title"].Value;
    var icon = element
        .QuerySelectorAll("span")
        .First(el => el.ClassName == "sp_radio__input-icon")
        .QuerySelectorAll("img")
        .OfType<IHtmlImageElement>()
        .First()
        .Source;
    var filterName = name.Replace(' ', '-').ToLower();
    return new Marker(name, filterName, icon);
}

public sealed record Marker(string Name, string FilterName, string IconUrl);

public sealed record MarkerGroup(string Name, Marker[] Markers);