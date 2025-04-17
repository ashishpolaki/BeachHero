using System;

namespace Bokka
{
    public class PropertyConditionAttribute : BaseAttribute
    {
        public PropertyConditionAttribute(Type targetAttributeType) : base(targetAttributeType)
        {
        }
    }
}
