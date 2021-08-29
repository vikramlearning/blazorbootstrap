using BlazorBootstrap.Enums;

namespace BlazorBootstrap.Utilities
{
    public class BootstrapClassProvider
    {

        #region Button

        public string Button() => "btn";

        public string ButtonColor(Color color) => $"{Button()}-{ToColor(color)}";

        public string ButtonOutline(Color color) => color != Color.None ? $"{Button()}-outline-{ToColor(color)}" : $"{Button()}-outline";

        public string ButtonSize(Size size) => $"{Button()}-{ToSize(size)}";

        public string ButtonBlock() => $"{Button()}-block";

        public string ButtonActive() => "active";

        public string ButtonDisabled() => "disabled";

        public string ButtonLoading() => null;

        #endregion

        #region Enums

        public string ToSize(Size size)
        {
            return size switch
            {
                Size.ExtraSmall => "xs",
                Size.Small => "sm",
                Size.Medium => "md",
                Size.Large => "lg",
                Size.ExtraLarge => "xl",
                _ => null,
            };
        }

        public string ToColor(Color color)
        {
            return color switch
            {
                Color.Primary => "primary",
                Color.Secondary => "secondary",
                Color.Success => "success",
                Color.Danger => "danger",
                Color.Warning => "warning",
                Color.Info => "info",
                Color.Light => "light",
                Color.Dark => "dark",
                Color.Link => "link",
                _ => null,
            };
        }

        #endregion
    }
}
