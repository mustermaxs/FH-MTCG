namespace MTCG
{
  public class ServerConfig : IConfig
    {
        public override string Name => "ServerConfig";
        public override string FilePath { get; set;} = "config.json";
        public override string Section { get; protected set;} = "server";
        public string SERVER_IP { get; set; }
        public int SERVER_PORT { get; set; }
        public int BufferSize { get; set; }
    }
}

