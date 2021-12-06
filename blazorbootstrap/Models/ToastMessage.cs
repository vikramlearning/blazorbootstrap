using System;

namespace BlazorBootstrap
{
    public class ToastMessage
    {
        public Guid Id { get; private set; }
        public string ImageSource { get; set; }

        public IconName IconName { get; set; }

        public string CustomIconName { get; set; }

        public string Title { get; set; }

        public string HelpText { get; set; }

        public string Message { get; set; }

        public ToastMessage()
        {
            this.Id = Guid.NewGuid();
        }
    }
}
