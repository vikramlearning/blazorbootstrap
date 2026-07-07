namespace BlazorBootstrap;

public class EnumUtility<TEnum> where TEnum : Enum
{
    public static List<EnumItem> GetEnumItems()
    {
        return (from TEnum e in Enum.GetValues(typeof(TEnum))
                select new EnumItem
                {
                    Value = Convert.ToInt32(e),
                    Text = typeof(TEnum).GetDisplayName(e.ToString()) ?? e.ToString()
                }).ToList();
    }
}
