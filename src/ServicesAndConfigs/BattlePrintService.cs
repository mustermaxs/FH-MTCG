using System;
using System.Text;

namespace MTCG;

public class BattlePrintService : IService
{
    protected StringBuilder sb;
    public BattleConfig? battleConfig { get; set; } = null;
    public string Language { get; set; } = "english";
    protected string? res;
    protected Battle? battle = null;
    public BattlePrintService()
    {
        sb = new StringBuilder();
        res = string.Empty;
    }


    /// <summary>
    /// Replaces template with provided string vals.
    /// Short non-descriptive name because i don't wanna think about it.
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="keyValPairs"></param>
    /// <returns></returns>
    public string Txt(string templateName, params (string key, dynamic value)[] keyValPairs)
    {
        var pairs = keyValPairs.Select(x => ("#" + x.Item1, x.Item2)).ToArray();
        var template = battleConfig![templateName];        
        // return pairs.Aggregate(template, (current, pair) => current.Replace("#" + pair.Item1, pair.Item2));

        foreach (var pair in pairs)
        {
            template = template.Replace(pair.Item1, pair.Item2);
        }

        return template;
    }

    public string GetBattleLogAsTxt(Battle battleObj)
    {
        battle = battleObj;
        res += Txt("header", ("player1", battle.Player1.Name), ("player2", battle.Player2.Name));
        res += Txt("roundcount", ("rounds", battle.CountRounds.ToString()));
        res += Txt("points", ("points", "0"));
        res += BuildLogEntryString();

        res += !battle.IsDraw
            ? Txt("winner", ("winner", battle.Winner.Name))
            : battleConfig!["draw"];

        return res;
    }

    protected string BuildLogEntryString()
    {
        string logs = string.Empty;

        foreach (var entry in battle!.BattleLog)
        {
            var timeStamp = (DateTime)entry.TimeStamp;
            logs += Txt("timestamp", ("time", entry.TimeStamp.ToString()));
            logs += Txt("roundnbr", ("roundnbr", entry.RoundNumber.ToString()));
            logs += Txt("logcards",
                ("player1", battle.Player1.Name),
                ("card1", entry.CardPlayedPlayer1.Name),
                ("player2", battle.Player2.Name),
                ("card2", entry.CardPlayedPlayer2.Name));
            logs += $"Damage {entry.CardPlayedPlayer1.Damage} vs {entry.CardPlayedPlayer2.Damage}\n";
            logs += $"Element: {entry.CardPlayedPlayer1.Type} vs {entry.CardPlayedPlayer2.Type}\n";
            logs += $"Typ: {entry.CardPlayedPlayer1.Type} vs {entry.CardPlayedPlayer2.Type}\n";
            
            logs += $"{entry.ActionDescriptions}";
            logs += Txt("countcardsleft", ("player", entry.Player1.Name), ("count", entry.CountCardsLeftPlayer1.ToString()));
            logs += Txt("countcardsleft", ("player", entry.Player2.Name), ("count", entry.CountCardsLeftPlayer2.ToString()));

            logs += entry.IsDraw
                ? battleConfig!["rounddraw"]
                : Txt("roundwin", ("winner", entry.RoundWinner.Name));

            logs += "\n\n";
        }

        return logs;
    }

    private string PrintAction(BattleLogEntry entry)
    {
        throw new NotImplementedException();
    }
}