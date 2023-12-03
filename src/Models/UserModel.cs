using System;

namespace MTCG;

public class User : IModel
{
    public string Name {get; set;} = "Max";
    public string Bio {get; set;} = "Bio";
    public User(string name, string bio)
    {
        Name = name;
        Bio = bio;
    }
    public User() {}
}