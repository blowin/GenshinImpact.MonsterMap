using GenshinImpact.MonsterMap.Domain.Icons;
using Newtonsoft.Json;
using RestSharp;

namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public sealed class ApiDataLoader : IApiDataLoader
{
    private readonly string _cookie;

    public ApiDataLoader(string cookie)
    {
        ArgumentException.ThrowIfNullOrEmpty(cookie);
        _cookie = cookie;
    }

    public IEnumerable<Icon> Load()
    {
        var response = Execute(_cookie);
        if (response?.Content == null)
            throw new InvalidOperationException($"Can't extract api data for \'{_cookie}\'");
        
        var results = JsonConvert.DeserializeObject<GenshinApiData>(response.Content);
        if(results?.data == null)
            throw new InvalidOperationException($"Result from api should not be empty for \'{_cookie}\'");
        
        return results.data.Select(info => info.ToFileIcon());
    }

    private static RestResponse Execute(string cookie)
    {
        using var client = new RestClient("https://tools-wiki.biligame.com/wiki/getMapData");
        client.Options.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4098.3 Safari/537.36";
        var request = new RestRequest("https://tools-wiki.biligame.com/wiki/getMapData", Method.Post)
        {
            Timeout = -1
        };
        request.AddHeader("Origin", "https://wiki.biligame.com");
        request.AddHeader("Accept-Encoding", "gzip, deflate, br");
        request.AddHeader("Accept-Language", "zh-CN,zh;q=0.8");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        request.AddHeader("Accept", "*/*");
        request.AddHeader("Referer", "https://wiki.biligame.com/ys/%E5%8E%9F%E7%A5%9E%E5%9C%B0%E5%9B%BE%E5%B7%A5%E5%85%B7_%E5%85%A8%E5%9C%B0%E6%A0%87%E4%BD%8D%E7%BD%AE%E7%82%B9");
        request.AddHeader("Connection", "keep-alive");
        request.AddParameter("application/x-www-form-urlencoded", cookie, ParameterType.RequestBody);
        return client.Execute(request);
    }
}