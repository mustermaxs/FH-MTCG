using System;

namespace MTCG;

public class User : IModel
{
    public string Name { get; set; } = "Max";
    public string Bio { get; set; } = "Bio";
    public string Password { get; set; }
    public string Image { get; set; }
    public int Coins { get; set; }
    public int ID { get; set; }

    public User()
    {

    }
}