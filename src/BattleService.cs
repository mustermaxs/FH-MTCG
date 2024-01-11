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
        protected static async Task<User> WaitForOpponent()
        {
            var tcs = new TaskCompletionSource<User>();

            ThreadPool.QueueUserWorkItem(_ =>
            {

                    Console.WriteLine($"IN QUEUE: {PendingBattleRequests.Count}");

                    lock (battleLock)
                    {
                        if (PendingBattleRequests.Count >= 2)
                        {
                            tcs.SetResult(PendingBattleRequests.Dequeue());
                        }
                    }

                    // You can add a delay here to avoid busy-waiting and reduce CPU usage
                    Thread.Sleep(100);
            });

            return await tcs.Task;
        }

        protected static void AddToQueue(User user)
        {
            lock (battleLock)
            {
                Console.WriteLine($"ADDED TO LOBBY {user.Name}");
                PendingBattleRequests.Enqueue(user);
            }
        }

        public static async Task<string> HandleBattle(User player1)
        {
            AddToQueue(player1);
            await WaitForOpponent();
            StartBattle();
        }

        public async 

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
