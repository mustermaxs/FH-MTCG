namespace MTCG
{
  public interface IUrlParams
    {
        string this[string key] { get; }
        Dictionary<string, string> QueryString { get; }
        Dictionary<string, string> NamedParams { get; }
    }
}
