using Screna;
using Screna.Avi;
using Screna.NAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoba.Models
{
    public class VideoCaptureEngine
    {
        private static VideoCaptureEngine instance = new VideoCaptureEngine();

        public static VideoCaptureEngine GetInstance() { return instance; }

        private VideoCaptureEngine()
        {
            // Initialize
        }

        private Recorder recorder = null;

        public int FrameRate
        {
            get
            {
                return Properties.Settings.Default.FrameRate;
            }
            set
            {
                Properties.Settings.Default.FrameRate = value;
                Properties.Settings.Default.Save();
            }
        }

        public MediaType MediaType
        {
            get
            {
                return (MediaType) Properties.Settings.Default.VideoType;
            }
            set
            {
                Properties.Settings.Default.VideoType = (int) value;
                Properties.Settings.Default.Save();
            }
        }

        public bool Recording { get { return recorder?.State == RecorderState.Recording; } }

        private string path = string.Empty; // recorder.RecordingStopped が fire しねえので仕方ない

        public void Start()
        {
            if (Recording) recorder?.Stop();

            var game_area = GameArea.GetInstance();

            if (!game_area.Screen.Primary)
                throw new Exception("Video capture is supported in primary screen.");

            if (game_area.Rects.Length == 0)
                throw new Exception("First of all, detect Game Area.");

            switch (MediaType)
            {
                case MediaType.Avi:
                    path = MediaPathProvider.Generate(MediaType.Avi);
            
                    var aviWriter = new AviWriter(path, AviCodec.MotionJpeg);
                    var videoProvider = new RegionProvider(game_area.Rects[0]);
                    var audioProvider = new LoopbackProvider();

                    recorder = new Recorder(aviWriter, videoProvider, FrameRate, audioProvider);

                    // recorder.RecordingStopped += (sender, args) =>
                    // {
                    //      NotificationProvider.Toast(
                    //          $"{path} is saved.",
                    //          "Aoba.png",
                    //          (s, a) => { Process.Start("explorer", $"/select,\"{path}\""); }
                    //      );
                    // };

                    recorder.Start();
                    break;

                case MediaType.Gif:
                    path = MediaPathProvider.Generate(MediaType.Gif);

                    var gifWriter = new GifWriter(path);
                    var imageProvider = new RegionProvider(game_area.Rects[0]);

                    recorder = new Recorder(gifWriter, imageProvider, FrameRate);
                    
                    recorder.Start();
                    break;
            }
        }

        public void Stop()
        {
            if (!Recording) return;

            recorder?.Stop();
            
            NotificationProvider.Toast(
                $"{path} is saved.",
                "Aoba.png",
                (s, a) => { Process.Start("explorer", $"/select,\"{path}\""); }
            );
        }
    }
}
