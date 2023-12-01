using System;
//IMPROVE could check if class implements IController interface
namespace MTCG
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ControllerAttribute : Attribute
    {
    }
}