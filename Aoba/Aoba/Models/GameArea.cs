using Screna;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aoba.Models
{
    public sealed class GameArea
    {
        private static GameArea instance = new GameArea();

        public static GameArea GetInstance()
        {
            return instance;
        }

        private GameArea()
        {
            Screen = Screen.PrimaryScreen;
        }

        private Screen screen; 

        public Screen Screen
        {
            get { return screen; }
            set
            {
                if (screen == value) return;

                screen = value;

                Rects = null; // Reset
            }
        }

        public Rectangle[] Rects { get; private set; }

        public void Detect(Screen screen)
        {
            var result = new List<Rectangle>();
            var desktop = ScreenShot.Capture(screen);

            for (int x = 1; x < desktop.Width - 1; x++)
            {
                for (int y = 1; y < desktop.Height - 1; y++)
                {
                    if (desktop.GetPixel(x, y).Name != "ffffffff" &&
                        desktop.GetPixel(x - 1, y).Name == "ffffffff" &&
                        desktop.GetPixel(x, y - 1).Name == "ffffffff" &&
                        desktop.GetPixel(x - 1, y - 1).Name == "ffffffff")
                    {
                        int w = 0;
                        try
                        {
                            while (desktop.GetPixel(x + w, y).Name != "ffffffff")
                            {
                                w++;
                            }

                            if (w < 800) continue;
                        }
                        catch
                        {
                            continue;
                        }

                        int h = 0;

                        try
                        {
                            while (desktop.GetPixel(x, y + h).Name != "ffffffff")
                            {
                                h++;
                            }

                            if (h < 480) continue;
                        }
                        catch
                        {
                            continue;
                        }

                        if (desktop.GetPixel(x + w + 1, y).Name == "ffffffff" &&
                            desktop.GetPixel(x, y + h + 1).Name == "ffffffff" &&
                            desktop.GetPixel(x + w + 1, y + h + 1).Name == "ffffffff")
                        {
                            // if (w > h) return new Rectangle(x, y, w, h);

                            if (w > h) result.Add(new Rectangle(x, y, w, h));
                        }
                    }
                }
            }

            if (result.Count > 0)
            {
                NotificationProvider.Toast("Game Area is detected successfully.", "Aoba.png");
            }
            else
            {
                NotificationProvider.Toast("Game Area is not found.", "Aoba.png");
            }

            Rects = result.ToArray();
        }
    }
}
