using System;
using UnityEngine;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SkinPickerAttribute : PropertyAttribute
    {
        public Type DatabaseType { get; private set; }

        public SkinPickerAttribute() { }

        public SkinPickerAttribute(Type databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}
