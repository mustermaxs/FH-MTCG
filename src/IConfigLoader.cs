namespace MTCG
{
  public interface IConfigLoader
  {
    public T? LoadConfig<T>(string filePath, string? section) where T : IConfig, new();
  }
}


