using System;

namespace MTCG;

public class BattleConfig : IConfig
{
    public override string Name => "BattleConfig";
    public override string Section { get; protected set; }
    public int MaxNbrRounds { get; set; }
    public BattleConfig()
    {
        this.Section = "battle";
    }

}