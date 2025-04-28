using System;
using System.ComponentModel;
using System.Reflection;

namespace M_21_31.Logger.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            if (field != null)
            {
                var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                if (attr != null)
                {
                    return attr.Description;
                }
            }

            return value.ToString(); // fallback to enum name
        }
    }
}
