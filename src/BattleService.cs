using System;
using System.Collections.Concurrent;
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
        private static object battleLock1 = new object();
        private static object battleLock2 = new object();
        private static ConcurrentDictionary<User, TaskCompletionSource<BattleLogEntry>> pendingBattles = new ConcurrentDictionary<User, TaskCompletionSource<BattleLogEntry>>();
        private static SemaphoreSlim foundOpponent = new SemaphoreSlim(0, int.MaxValue);

        public static ConcurrentQueue<User> pendingUsers = new ConcurrentQueue<User>();

        public BattleService()
        {
        }

        /// <summary>
        /// Adds user id to queue. when theres two ids in the queue
        /// it returns them.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected static TaskCompletionSource<BattleLogEntry> AddToWaitingLine(User player)
        {
            lock (battleLock1)
            {
                var waitingTask = new TaskCompletionSource<BattleLogEntry>();
                pendingBattles[player] = waitingTask;
                OnFindOpponentTriggerStart();

                return waitingTask;
            }
        }

        protected static void AddToQueue(User user)
        {
            lock (battleLock1)
            {
                Console.WriteLine($"ADDED TO LOBBY {user.Name}");
                pendingUsers.Enqueue(user);
            }

        }

        public static void OnFindOpponentTriggerStart()
        {
            if (pendingUsers.Count >= 2 && pendingUsers.Count != 1)
            {
                Console.WriteLine($"Found opponent.");
                foundOpponent.Release();
            }
        }

        public static async Task<BattleLogEntry> BattleRequest(User player1)
        {
            AddToQueue(player1);
            var res = AddToWaitingLine(player1);
            PerformBattle();

            BattleLogEntry battleResult = await res.Task;

            return battleResult;
        }

        public static async void PerformBattle()
        {
            await foundOpponent.WaitAsync();

            lock (battleLock1)
            {
                if (pendingUsers.TryDequeue(out User? player1) &&
                        pendingUsers.TryDequeue(out User? player2))

                {
                    List<string> actions = new List<string>();
                    actions.Add($"STARTING BATTLE: {player1.Name} vs {player2.Name}");

                    var battle = new BattleLogEntry { Player1 = player1, Player2 = player2 };

                    pendingBattles[player1].SetResult(battle);
                    pendingBattles[player2].SetResult(battle);

                    pendingBattles.TryRemove(player1, out _);
                    pendingBattles.TryRemove(player2, out _);
                }
            }


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