using System;
using System.Text.Json;
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

        public ScoreboardController(IRequest context):base(context) {}


        [Route("score", HTTPMethod.GET)]
        public string GetScore()
        {
            return JsonSerializer.Serialize<Point>(new Point("Max", 100));
        }
    }

}