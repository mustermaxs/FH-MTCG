namespace MTCG
{
  public class UrlParams : IUrlParams
    {
        public Dictionary<string, string> QueryString => queryStringParams;
        private Dictionary<string, string> queryStringParams;
        public Dictionary<string, string> NamedParams => namedParams;
        private Dictionary<string, string> namedParams;

        public UrlParams(Dictionary<string, string>? namedParams, Dictionary<string, string>? queryStringParams)
        {
            this.namedParams = namedParams ?? new Dictionary<string, string>();
            this.queryStringParams = queryStringParams ?? new Dictionary<string, string>();
        }
        public UrlParams()
        {
            this.namedParams = new Dictionary<string, string>();
            this.queryStringParams = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                if (namedParams.TryGetValue(key, out var value))
                {
                    return value;
                }
                return string.Empty;
            }

            set
            {
                if (namedParams.ContainsKey(key))
                {
                    namedParams[key] = value;
                }
                else
                {
                    namedParams.Add(key, value);
                }
            }
        }
    }
}
