using System;

namespace MTCG;

public class User : IModel
{
    virtual public string Name { get; set; } = string.Empty;
    virtual public string Bio { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int Coins { get; set; }
    private Role UserRole = Role.USER;
    public Role GetUserAccessLevel() => UserRole;
    public void SetUserAccessLevel(Role role) => UserRole = role;
    public Guid ID { get; set; }

    public User()
    {
    }
}