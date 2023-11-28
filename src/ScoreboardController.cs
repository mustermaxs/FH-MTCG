using System;
using MTCG;

namespace MTCG
{
    public class Point
    {
        public Point(string user, int value)
        {
            User = user;
            Val = value;
        }
        public string? User { get; set; }
        public int Val { get; set; } = 0;
    }

    [Controller]
public class ScoreboardController : IController
{
    private Point score;

    public ScoreboardController()
    {
        // Initialize score in the constructor if you want a single instance
        score = new Point("Max", 100);
    }

    public Point Score
    {
        get
        {
            // You can still use Score as a property without a routing attribute
            return score;
        }
    }

    [Route("score", HTTPMethod.GET)]
    public Point GetScore()
    {
        // You can put logic here if needed
        return score;
    }
}

}