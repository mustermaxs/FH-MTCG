using System;
//IMPROVE return controller instance?
namespace MTCG
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ControllerAttribute : Attribute
    {
    }
}