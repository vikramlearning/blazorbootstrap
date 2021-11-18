using System;

namespace BlazorBootstrap
{
    public class OffcanvasService
    {
        public event Action<string> OnShow;
        public event Action<string> OnHide;

        public void Show(string elementId) => OnShow?.Invoke(elementId);
        public void Hide(string elementId) => OnHide?.Invoke(elementId);
    }
}
