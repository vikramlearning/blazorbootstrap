using System.ComponentModel;

namespace BlazorBootstrap;

public class EnumUtility<TEnum> where TEnum : Enum
{
    public static List<EnumItem> GetEnumItems()
    {
        return (from TEnum e in Enum.GetValues(typeof(TEnum))
                select new EnumItem
                {
                    Value = Convert.ToInt32(e),
                    Text = GetEnumDescription(e)
                }).ToList();
    }

    private static string GetEnumDescription(TEnum value)
    {
        if (!(value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false)?.FirstOrDefault() is DescriptionAttribute descriptionAttribute))
        {
            return value.ToString();
        }

        return descriptionAttribute.Description;
    }
}
