using System;
//IMPROVE return controller instance?


namespace MTCG;

/// <summary>
/// Marks a class as a controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ControllerAttribute : Attribute
{
}