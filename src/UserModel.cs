using System;

namespace MTCG;

public class UserModel : IModel
{
    public string Name {get; set;} = "Max";
    public string Bio {get; set;} = "Bio";
    public UserModel(string name, string bio)
    {
        Name = name;
        Bio = bio;
    }
    public UserModel() {}
}