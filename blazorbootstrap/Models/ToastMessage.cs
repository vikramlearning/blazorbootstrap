﻿using System;

namespace BlazorBootstrap
{
    public class ToastMessage : IEquatable<ToastMessage>
    {
        public Guid Id { get; private set; }

        public ToastType Type { get; set; }

        public string ImageSource { get; set; }

        public IconName IconName { get; set; }

        /// <summary>
        /// Specify custom icons of your own, like fontawesome. Example: 'fas fa-alarm-clock'
        /// </summary>
        public string CustomIconName { get; set; }

        public string Title { get; set; }

        public string HelpText { get; set; }

        public string Message { get; set; }

        public ToastMessage()
        {
            this.Id = Guid.NewGuid();
        }

        public bool Equals(ToastMessage other)
        {
            if(other == null) return false;
            return this.Id.Equals(other.Id);
        }
    }
}
