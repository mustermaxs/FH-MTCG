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
        private static ConcurrentDictionary<User, TaskCompletionSource<Battle>> pendingBattles = new ConcurrentDictionary<User, TaskCompletionSource<Battle>>();
        private static SemaphoreSlim foundOpponent = new SemaphoreSlim(0, int.MaxValue);

        public static ConcurrentQueue<User> pendingUsers = new ConcurrentQueue<User>();

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
        protected static TaskCompletionSource<Battle> AddToWaitingLine(User player)
        {
            lock (battleLock1)
            {
                var waitingTask = new TaskCompletionSource<Battle>();
                pendingBattles[player] = waitingTask;
                OnFindOpponent();

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

        public static void OnFindOpponent()
        {
            if (pendingUsers.Count >= 2 && pendingUsers.Count != 1)
            {
                Console.WriteLine($"Found opponent.");
                foundOpponent.Release();
            }
        }

        public static async Task<Battle> HandleBattle(User player1)
        {
            AddToQueue(player1);
            var res = AddToWaitingLine(player1);
            StartBattle();

            Battle battleResult = await res.Task;

            return battleResult;
        }

        public static async void StartBattle()
        {
            await foundOpponent.WaitAsync();

            lock (battleLock1)
            {
                if (pendingUsers.TryDequeue(out User? player1) &&
                        pendingUsers.TryDequeue(out User? player2))

                {
                    Console.WriteLine($"STARTING BATTLE: {player1.Name} vs {player2.Name}");
                    List<string> actions = new List<string>();
                    actions.Add($"STARTING BATTLE: {player1.Name} vs {player2.Name}");

                    var battle = new Battle { Player1 = player1, Player2 = player2 };

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