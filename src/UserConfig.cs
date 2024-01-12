namespace MTCG
{
  public class UserConfig : IConfig
    {
        public override string Name => "UserConfig";
        public override string Section { get;  protected set;} = "user";
        // public int StartAmountCoins { get; set; }
        // public UserConfig(IConfigLoader configLoader) :base(configLoader)
        // {
          
        // }
    }
}

