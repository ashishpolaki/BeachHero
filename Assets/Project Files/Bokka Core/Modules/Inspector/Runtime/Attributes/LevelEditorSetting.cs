using System;

namespace Bokka
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LevelEditorSetting : GroupAttribute
    {
        public LevelEditorSetting() : base("System", "", 0)
        {

        }
    }
}
