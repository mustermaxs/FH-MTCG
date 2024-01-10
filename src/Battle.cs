using System;


namespace MTCG;

public class Battle
{
    object pendingBattlesLock = new object();

    public static Queue<BattleAction> PendingBattleRequests = new Queue<BattleAction>();

    
    public Battle() {}


    
}