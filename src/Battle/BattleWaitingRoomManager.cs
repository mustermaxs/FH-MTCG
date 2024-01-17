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

    public class BattleWaitingRoomManager
    {
        // public event EventHandler<BattleEventArgs> StartBattle;
        private static object battleLock1 = new object();
        private static object battleLock2 = new object();
        private static ConcurrentDictionary<User, TaskCompletionSource<Battle>> pendingBattles = new ConcurrentDictionary<User, TaskCompletionSource<Battle>>();
        private static SemaphoreSlim foundOpponent = new SemaphoreSlim(0, int.MaxValue);

        public static ConcurrentQueue<User> waitingUsers = new ConcurrentQueue<User>();

        public BattleWaitingRoomManager()
        {
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
                OnFindOpponentTriggerStart();

                return waitingTask;
            }
        }

        protected static void AddToQueue(User user)
        {
            lock (battleLock1)
            {
                Console.WriteLine($"ADDED TO LOBBY {user.Name}");
                waitingUsers.Enqueue(user);
            }

        }

        public static void OnFindOpponentTriggerStart()
        {
            if (waitingUsers.Count >= 2 && waitingUsers.Count != 1)
            {
                Console.WriteLine($"Found opponent.");
                foundOpponent.Release();
            }
        }

        public static async Task<Battle> BattleRequest(User player1)
        {
            AddToQueue(player1);
            var res = AddToWaitingLine(player1);
            PerformBattle();

            Battle battleResult = await res.Task;

            return battleResult;
        }

        public static async void PerformBattle()
        {
            await foundOpponent.WaitAsync();

            lock (battleLock1)
            {
                if (waitingUsers.TryDequeue(out User? player1) &&
                        waitingUsers.TryDequeue(out User? player2))

                {
                    string battleToken = CryptoHandler.GenerateRandomString(10);
                    List<string> actions = new List<string>();
                    var battleConfig = ServiceProvider.Get<BattleConfig>();
                    battleConfig = battleConfig.Load<BattleConfig>();
                    var battleManager = new BattleManager(player1, player2, battleConfig);
                    battleManager.SetBattleToken(battleToken);
                    battleManager.UseCardRepo(ServiceProvider.GetFreshInstance<CardRepository>());

                    var battle = battleManager.Play();
                    
                    pendingBattles[player1].SetResult(battle);
                    pendingBattles[player2].SetResult(battle);

                    pendingBattles.TryRemove(player1, out _);
                    pendingBattles.TryRemove(player2, out _);

                    return;
                }
            }


        }
    }
}