namespace BlazorBootstrap.Extensions
{
    public static class EnumExtensions
    {
        public static string ToButtonTagName(this ButtonType buttonType)
        {
            return buttonType switch
            {
                ButtonType.Link => "a",
                _ => "button"
            };
        }

        public static string ToButtonTypeString(this ButtonType buttonType)
        {
            return buttonType switch
            {
                ButtonType.Button => "button",
                ButtonType.Submit => "submit",
                ButtonType.Reset => "reset",
                _ => null,
            };
        }
    }
}
