namespace MTCG
{
  public abstract class IConfig
    {
        public abstract string Name { get; }
        public virtual string FilePath { get; } = "config.json";
        public abstract string Section { get; }
    }
}

