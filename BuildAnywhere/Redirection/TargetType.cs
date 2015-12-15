using System;

namespace BuildAnywhere.Redirection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TargetType : Attribute
    {
        public TargetType(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}