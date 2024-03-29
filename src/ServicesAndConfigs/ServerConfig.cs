namespace MTCG
{
  public class ServerConfig : BaseConfig, IService
    {
        public override string Name => "ServerConfig";
        public override string FilePath { get; set;} = "config.json";
        public override string Section { get; protected set;} = "server";
        public string SERVER_IP { get; set; }
        public int SERVER_PORT { get; set; }
        public int BufferSize { get; set; }
        public ServerConfig()
        {
            SERVER_IP = string.Empty;
            SERVER_PORT = 0;
            BufferSize = 0;
        }
        // public ServerConfig(IConfigLoader configLoader) :base(configLoader)
        // {
        //     SERVER_IP = string.Empty;
        //     SERVER_PORT = 0;
        //     BufferSize = 0;
        // }
        
    }
}

