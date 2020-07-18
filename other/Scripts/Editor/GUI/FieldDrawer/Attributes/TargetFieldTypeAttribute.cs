using System;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TargetFieldType : Attribute
    {
        public Type TargetType { get; set; }
        public TargetFieldType(Type type)
        {
            TargetType = type;
        }
    }
}
