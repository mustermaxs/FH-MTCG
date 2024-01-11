using System;
using System.Collections.Generic;

namespace MTCG
{
    public class BattleEventArgs : EventArgs
    {
        public Guid Player1 { get; }
        public Guid Player2 { get; }

        public BattleEventArgs(Guid player1, Guid player2)
        {
            Player1 = player1;
            Player2 = player2;
        }
    }

    public class BattleService
    {
        // public event EventHandler<BattleEventArgs> StartBattle;
        private static object battleLock = new object();

        public static Queue<User> PendingBattleRequests = new Queue<User>();

        public BattleService()
        {
            // StartBattle += (sender, args) => { };
        }

        /// <summary>
        /// Adds user id to queue. when theres two ids in the queue
        /// it returns them.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected async Task<User> WaitForOpponent()
        {
            return await Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine($"IN QUEUE: {PendingBattleRequests.Count}");

                    if (PendingBattleRequests.Count >= 2)
                    {
                        return PendingBattleRequests.Dequeue();
                    }
                }
            });
        }

        protected void AddToQueue(User user)
        {
            lock (battleLock)
            {
                Console.WriteLine($"ADDED TO LOBBY {user.Name}");
                PendingBattleRequests.Enqueue(user);
            }
        }

        public async Task<string> HandleBattle(User player1)
        {
            Logger.ToConsole($"Player {player1.Name} entered the lobby.");
            // AddToQueue(player1);
            // var player2 = await Task.Delay(10000);
            await Task.Delay(10000);
            // Logger.ToConsole($"{player1.Name} vs. {player2.Name}");
            
            return $"{player1.Name} vs.";
        }

        // protected virtual void OnReadyForBattle(object sender, BattleEventArgs e)
        // {
        //     lock (battleLock)
        //     {
        //         if (PendingBattleRequests.Count >= 2)
        //         {
        //             StartBattle?.Invoke(this, e);
        //         }
        //     }
        // }
    }
}
