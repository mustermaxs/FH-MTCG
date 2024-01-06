namespace MTCG
{
  public class ServerConfig : IConfig
    {
        public override string Name => "ServerConfig";
        public override string FilePath => "./config.json";
        public override string Section => "server";
    }
}

