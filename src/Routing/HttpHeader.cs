using System;



namespace MTCG
{
    /// <summary>This class represents a HTTP header.</summary>
    public class HttpHeader
    {
        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="header">Header text.</param>
        /// 

        public HttpHeader()
        {

        }


        public HttpHeader(string header)
        {
            Name = Value = string.Empty;

            try
            {
                int n = header.IndexOf(':');
                Name = header.Substring(0, n).Trim();
                // if is authoriation token, save only value of token as value without the auth schema
                if (header.Contains("Authorization"))
                {
                    Value = ExtractAuthToken(header);
                    return;
                }
                Value = header.Substring(n + 1).Trim();
            }
            catch (Exception) { }
        }

        protected string ExtractAuthToken(string authToken)
        {
            return authToken.Split(' ')[2];
        }




        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the header name.</summary>
        virtual public string Name
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the header value.</summary>
        virtual public string Value
        {
            get; protected set;
        } = string.Empty;
    }
}
