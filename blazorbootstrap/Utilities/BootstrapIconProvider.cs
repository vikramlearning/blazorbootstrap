namespace BlazorBootstrap.Utilities
{
    public class BootstrapIconProvider
    {
        #region Bootstrap Icons

        public string Icon() => "bi";

        public string Icon(IconName iconName) => $"{Icon()}-{ToIconName(iconName)}";

        #endregion

        #region Methods

        public string ToIconName(IconName iconName)
        {
            return iconName switch
            {
                BlazorBootstrap.IconName.Alarm => "alarm",
                BlazorBootstrap.IconName.AlarmFill => "alarm-fill",
                // TODO: add all the icons
                _ => null,
            };
        }

        public string IconSize(IconSize iconSize)
        {
            return iconSize switch
            {
                BlazorBootstrap.IconSize.x2 => "fs-2",
                BlazorBootstrap.IconSize.x3 => "fs-3",
                BlazorBootstrap.IconSize.x4 => "fs-4",
                BlazorBootstrap.IconSize.x5 => "fs-5",
                _ => null,
            };
        }

        #endregion
    }
}
