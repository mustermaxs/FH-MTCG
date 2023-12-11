using System;

namespace MTCG;

public class User : IModel
{
    virtual public string Name { get; set; } = string.Empty;
    virtual public string Bio { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Coins { get; set; }
    public Guid ID { get; set; }

    public User()
    {
    }
}