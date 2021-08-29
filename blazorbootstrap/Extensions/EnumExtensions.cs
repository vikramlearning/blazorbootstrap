using BlazorBootstrap.Enums;

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

        /// <summary>
        /// Gets the link target name.
        /// </summary>
        public static string ToTargetString(this Target target) => target switch
        {
            Target.Blank => "_blank",
            Target.Parent => "_parent",
            Target.Top => "_top",
            Target.Self => "_self",
            _ => null,
        };
    }
}
