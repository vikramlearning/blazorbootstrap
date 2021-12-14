using System;

namespace BlazorBootstrap
{
    public class PageLoadingService
    {
        public event Action OnShow;
        public event Action OnHide;

        public void Show() { OnShow.Invoke(); }
        public void Hide() { OnHide.Invoke(); }
    }
}
