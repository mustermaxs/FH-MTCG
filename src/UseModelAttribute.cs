using System;
using MTCG;


namespace MTCG
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UseModelAttribute : Attribute
    {
        public readonly string ModelName;

        public UseModelAttribute(string modelName)
        {
            this.ModelName = modelName;
        }
    }
}