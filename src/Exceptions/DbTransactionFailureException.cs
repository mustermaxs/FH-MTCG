using System;

namespace MTCG
{
    public class DbTransactionFailureException : Exception
    {
        private string invalidUrl;

        public string Url => invalidUrl;

        public DbTransactionFailureException(string invalidUrl)
        {
            this.invalidUrl = invalidUrl;
        }

        public DbTransactionFailureException(string invalidUrl, string message)
            : base(message)
        {
            this.invalidUrl = invalidUrl;
        }

        public DbTransactionFailureException(string invalidUrl, string message, Exception inner)
            : base(message, inner)
        {
            this.invalidUrl = invalidUrl;
        }
    }
}
