using System;

namespace Bokka
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReorderableListAttribute : Attribute
    {
    }
}
