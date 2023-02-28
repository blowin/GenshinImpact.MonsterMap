using System.Diagnostics;

namespace GenshinImpact.MonsterMap.Domain.GameProcesses.GameProcessProviders;

public interface IGameProcessProvider
{
    IGameProcess GetProcess();
}