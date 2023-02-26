using System;

namespace GenshinImpact.MonsterMap.Script;

/// <summary>
/// The running time of each module prints the timing class
/// </summary>
class Timer
{
    static DateTime startTime;
    public static void Init() => startTime = DateTime.Now;
    public static void Show(string text) => Console.WriteLine(text + (DateTime.Now - startTime));
}