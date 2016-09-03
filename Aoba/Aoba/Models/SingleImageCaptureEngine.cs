using Screna;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoba.Models
{
    public sealed class SingleImageCaptureEngine
    {
        private static SingleImageCaptureEngine instance = new SingleImageCaptureEngine();

        public static SingleImageCaptureEngine GetInstance() { return instance; }

        private SingleImageCaptureEngine()
        {
            // MediaType を変更できるようにする？
        }

        public bool Save()
        {
            var game_area = GameArea.GetInstance();
            var path = MediaPathProvider.Generate(MediaType.Png);

            if (game_area.Rects.Length == 0)
                throw new Exception("First of all, detect Game Area.");

            using (var bitmap = ScreenShot.Capture(game_area.Screen))
            {
                try
                {
                    bitmap
                        .Clone(game_area.Rects[0], bitmap.PixelFormat)
                        .Save(path);

                    NotificationProvider.Toast(
                        $"{path} is saved.",
                        path,
                        (s, a) => { Process.Start("explorer", $"/select,\"{path}\""); }
                    );

                    return true;
                }
                catch(Exception e)
                {
                    NotificationProvider.Toast(e.Message, "Aoba.png");

                    return false;
                }
            }
        }
    }
}
