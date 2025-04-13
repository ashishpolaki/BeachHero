using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DefineAttribute : Attribute
    {
        public string Define { get; private set; }
        public string AssemblyType { get; private set; }
        public string[] LinkedFiles { get; private set; }

        public DefineAttribute(string define, string assemblyType = "", string[] linkedFiles = null)
        {
            Define = define;

            AssemblyType = assemblyType;
            LinkedFiles = linkedFiles;
        }
    }
}
