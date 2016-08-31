﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace Aoba.Models
{
    public sealed class NotifyProviderModel
    {
        private static NotifyProviderModel instance = new NotifyProviderModel();

        public static NotifyProviderModel GetInstance()
        {
            return instance;
        }

        public NotifyProviderModel()
        {
            IsEnabled = true;
        }

        public void Toast(string message, string image_path,
            TypedEventHandler<ToastNotification, object> activated = null,
            TypedEventHandler<ToastNotification, ToastDismissedEventArgs> dismissed = null,
            TypedEventHandler<ToastNotification, ToastFailedEventArgs> failed = null)
        {
            if (!IsEnabled) return;

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(message));

            var imagePath = "file:///" + Path.GetFullPath(image_path);
            var imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            ToastNotification toast = new ToastNotification(toastXml);
            toast.Activated += activated;
            toast.Dismissed += dismissed;
            toast.Failed += failed;

            const string APP_ID = "Daruyanagi.Aoba";

            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        }
        
        public bool IsEnabled { get; set; }
    }
}
