namespace GenshinImpact.MonsterMap.Domain.Api.Loaders;

public interface IApiDataLoader
{
    IEnumerable<FileIcon> Load();
}