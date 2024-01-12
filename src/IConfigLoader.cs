namespace MTCG
{
  public interface IConfigLoader
  {
    public T? LoadConfig<T>(string filePath, string? key) where T : IConfig, new();
  }
}


