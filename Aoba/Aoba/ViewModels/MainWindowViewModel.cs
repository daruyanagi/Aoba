using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Windows.UI.Notifications;

namespace Aoba.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public ICommand DetectCommand { get; private set; }
        public ICommand CaptureCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }

        public MainWindowViewModel()
        {
            DetectCommand = new DelegateCommand(_ => {
                var bitmap = CaptureDesktop(SelectedDesktop);

                try
                {
                    Rectangle = DetectGameArea(bitmap);
                    ClearErrror("DetectGameArea()");
                    CanCapture = true;
                }
                catch (Exception e)
                {
                    SetError("DetectGameArea()", e.Message);
                    CanCapture = false;
                }
            });

            CaptureCommand = new DelegateCommand(_ =>
            {
                var bitmap = CaptureDesktop(SelectedDesktop);

                bitmap = bitmap.Clone(Rectangle, bitmap.PixelFormat);

                var path = GenerateFilePath();
                bitmap.Save(path);

                if (notify)
                {
                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                    var stringElements = toastXml.GetElementsByTagName("text");
                    stringElements[0].AppendChild(toastXml.CreateTextNode(path + " is saved."));

                    var imagePath = "file:///" + Path.GetFullPath(path);
                    var imageElements = toastXml.GetElementsByTagName("image");
                    imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                    ToastNotification toast = new ToastNotification(toastXml);
                    toast.Activated += (sender, args) => { System.Diagnostics.Process.Start("explorer", @"/select," + path); };
                    toast.Dismissed += (sender, args) => { };
                    toast.Failed += (sender, args) => { };

                    const string APP_ID = "Daruyanagi.Aoba";

                    ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
                }
            }, _ => CanCapture);

            OpenCommand = new DelegateCommand(_ =>
            {
                System.Diagnostics.Process.Start(StoragePath);
            });
        }

        private Rectangle rectangle;

        public Rectangle Rectangle
        {
            get { return rectangle; }
            set { SetProperty(ref rectangle, value); }
        }

        private bool canCapture = false;

        public bool CanCapture
        {
            get { return canCapture; }
            set { SetProperty(ref canCapture, value); }
        }

        private bool notify = true;

        public bool Notify
        {
            get { return notify; }
            set { SetProperty(ref notify, value); }
        }

        public Screen[] Desktops
        {
            get { return Screen.AllScreens; }
        }

        private int selectedDesktop = 0;

        public int SelectedDesktop
        {
            get { return selectedDesktop; }
            set { SetProperty(ref selectedDesktop, value); }
        }

        public string StoragePath
        {
            get
            {
                var path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                path = Path.Combine(path, "Aoba");

                path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd"));

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        public string GenerateFilePath()
        {
            var path = StoragePath;

            path = System.IO.Path.Combine(path, DateTime.Now.ToString("HHmmss-ff"));
            path = System.IO.Path.ChangeExtension(path, "png");

            return path;
        }

        private static Bitmap CaptureDesktop(int screen = 0)
        {
            var rect = Screen.AllScreens[screen].Bounds;

            var bitmap = new Bitmap(
                rect.Width, rect.Height, 
                PixelFormat.Format32bppArgb
            );

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(
                    rect.X, rect.Y, 0, 0, rect.Size, 
                    CopyPixelOperation.SourceCopy
                );
            }

            return bitmap;
        }

        private static Rectangle DetectGameArea(Bitmap desktop)
        {
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
                            return new Rectangle(x, y, w, h);
                        }
                    }
                }
            }

            throw new Exception("Game Area is not found.");
        }
    }
}
