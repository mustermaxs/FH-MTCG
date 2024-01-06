namespace MTCG
{
  public abstract class IConfig
  {
    public abstract string Name { get; }
    public virtual string FilePath { get; set;} = "config.json";
    public abstract string Section { get; protected set;}
    // public static T Load<T>(string? filePath, string? section) where T : IConfig, new()
    // {

    // }

  }
}


