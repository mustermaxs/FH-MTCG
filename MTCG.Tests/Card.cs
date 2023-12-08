using System;

namespace MTCG;

public class Card : IModel
{
    public Guid Id { get; set; }
    public string Description {get; set;}
    public float Damage {get; set;}
    public string Name {get; set;}

}