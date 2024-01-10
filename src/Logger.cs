using System;

namespace MTCG;

public class Logger
{
    public readonly static string decoline = new string('-', 15);
    public static void ToConsole(string txt, bool withTimeStamp=false)
    {
        DateTime localDate = DateTime.Now;
        DateTime utcDate = DateTime.UtcNow;
        var timestamp = withTimeStamp ? localDate.ToString("yyyy-MM-dd HH:mm:ss") : "";

        string decoline = new string('-', 25);
        string logTxt = $"{decoline}\n{timestamp}\n{txt}";
        Console.WriteLine(logTxt);
    }
}