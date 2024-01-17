using System;

namespace MTCG;

public class Logger
{
    protected static string decoline = new string('-', 25);

    public static void ToConsole(string txt, bool withTimeStamp = false)
    {
        string logTxt = $"{decoline}\n{TimeStamp(null)}\n{txt}";
        Console.WriteLine(logTxt);
    }

    public static void Err(string txt, bool withTimeStamp = false)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        ToConsole($"[ERROR] {txt}", withTimeStamp);
        Console.ResetColor();
    }
    public static void Err(Exception ex, bool withTimeStamp = false)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        ToConsole($"[ERROR] {ex.ToString()}", withTimeStamp);
        Console.ResetColor();
    }


    public static string TimeStamp(DateTime? time)
    {
        string format = "yyyy-MM-dd HH:mm:ss";

        return time == null ? DateTime.Now.ToString(format) : time.Value.ToString(format);
    }
}