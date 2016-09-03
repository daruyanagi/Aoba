using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Aoba.Models
{
    public sealed class BurstImageCaptureEngine
    {
        private static BurstImageCaptureEngine instance = new BurstImageCaptureEngine();

        public static BurstImageCaptureEngine GetInstance() { return instance; }

        private BurstImageCaptureEngine()
        {
            timer.Tick += (sender, args) =>
            {
                var engine = SingleImageCaptureEngine.GetInstance();

                var result = engine.Save();

                if (!result) Stop();
            };
        }

        private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000), };

        public int Interval
        {
            get { return (int) timer.Interval.TotalMilliseconds; }
            set { timer.Interval = TimeSpan.FromMilliseconds(value); }
        }

        public bool Recording
        {
            get { return timer.IsEnabled; }
        }

        private bool notify;

        public void Start()
        {
            NotificationProvider.Toast(
                $"Burst capture started.",
                "Aoba.png",
                (s, a) => {  }
            );

            notify = NotificationProvider.IsEnabled();
            NotificationProvider.Disable();

            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();

            if (notify)
                NotificationProvider.Enable();
            else
                NotificationProvider.Disable();

            var path = MediaPathProvider.GetPictureFolder();

            NotificationProvider.Toast(
                $"Burst capture stopped.",
                "Aoba.png",
                (s, a) => { Process.Start("explorer", $"/select,\"{path}\""); }
            );
        }
    }
}
