namespace MTCG
{
  public abstract class BaseConfig : IService
  {
    protected virtual IConfigLoader configLoader { get; set; } = new JsonConfigLoader();
    public abstract string Name { get; }
    public virtual string FilePath { get; set; } = "config.json";
    public abstract string Section { get; protected set; }
    public void SetConfigLoader(IConfigLoader loader) => this.configLoader = loader;
    // public IConfig(IConfigLoader configLoader)
    // {
    //   this.configLoader = configLoader;
    // }
    public virtual T Load<T>(string? filePath, string? keyword) where T : BaseConfig, new()
    {
      var path = filePath ?? new T().FilePath;
      var _keyword = keyword ?? new T().Section;
      var config = (T)configLoader.LoadConfig<T>(path, _keyword);

      if (config == default)
        throw new Exception("Failed to deserialize config file");

      return config;
    }
    public virtual T Load<T>() where T : BaseConfig, new()
    {
      return Load<T>(null, null);
    }
    public virtual T Load<T>(string filePath) where T : BaseConfig, new()
    {
      return Load<T>(filePath, null);
    }

  }
}


