using System;

namespace MTCG;

public class User : IModel
{
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Coins { get; set; }
    public int ID { get; set; }

    public User()
    {

    }
}