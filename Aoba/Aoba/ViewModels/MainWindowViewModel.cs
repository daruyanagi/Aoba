using Screna;
using Screna.Audio;
using Screna.Avi;
using Screna.NAudio;
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
        public ICommand SingleCaptureCommand { get; private set; }
        public ICommand BurstCaptureCommand { get; private set; }
        public ICommand VideoCaptureCommand { get; private set; }
        public ICommand OpenPictureFolderCommand { get; private set; }
        public ICommand OpenVideoFolderCommand { get; private set; }

        Timer BurstTimer = new Timer();
        Recorder recorder = null;
        string videoPath = string.Empty;

        public MainWindowViewModel()
        {
            BurstCaptureInterval = 1000;

            BurstTimer.Tick += (sender, args) =>
            {
                var path = GeneratePicturePath();

                try
                {
                    CaptureGameArea(path);
                }
                catch
                {
                    BurstTimer.Stop();
                }
            };

            DetectCommand = new DelegateCommand(_ => {
                var bitmap = CaptureDesktop(SelectedDesktop);

                try
                {
                    Rectangle = DetectGameArea(bitmap);
                    ClearErrror("DetectGameArea()");
                    CanCapture = true;

                    if (notify)
                    {
                        NotifyMessage("Game Area is detected successfully.");
                    }
                }
                catch (Exception e)
                {
                    SetError("DetectGameArea()", e.Message);
                    NotifyMessage(e.Message);
                    CanCapture = false;
                }
            });

            VideoCaptureCommand = new DelegateCommand(_ =>
            {
                // video - c# Screna: How do I define screen area? - Stack Overflow
                // http://stackoverflow.com/questions/35505744/c-sharp-screna-how-do-i-define-screen-area

                if (recorder == null)
                {
                    videoPath = GenerateVideoPath();
                    var writer = new AviWriter(videoPath, AviCodec.MotionJpeg);
                    var videoProvider = new RegionProvider(Rectangle);
                    var audioProvider = new LoopbackProvider();

                    recorder = new Recorder(writer, videoProvider, FrameRate, audioProvider);

                    recorder.Start();

                    VideoCaptureButtonBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);

                    if (notify)
                    {
                        //
                    }
                }
                else
                {
                    recorder.Stop();

                    recorder = null;

                    VideoCaptureButtonBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);

                    if (notify)
                    {
                        NotifyVideoSaved(videoPath);
                    }
                }
            }, _ => CanCapture && SelectedDesktop == 0);

            SingleCaptureCommand = new DelegateCommand(_ =>
            {
                var path = GeneratePicturePath();

                try
                {
                    CaptureGameArea(path);

                    if (notify)
                    {
                        NotifyScreenshotSaved(path);
                    }
                }
                catch (Exception e)
                {
                    NotifyMessage(e.Message);
                }
            }, _ => CanCapture);

            BurstCaptureCommand = new DelegateCommand(_ =>
            {
                if (BurstTimer.Enabled)
                {
                    BurstTimer.Stop();
                    BurstCaptureButtonBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);

                    if (notify)
                    {
                        NotifyMessage("Burst capture is stopped.");
                    }
                }
                else
                {
                    BurstTimer.Start();
                    BurstCaptureButtonBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);

                    if (notify)
                    {
                        NotifyMessage("Burst capture is started.");
                    }
                }
            }, _ => CanCapture);

            OpenPictureFolderCommand = new DelegateCommand(_ =>
            {
                System.Diagnostics.Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Aoba"));
            });

            OpenVideoFolderCommand = new DelegateCommand(_ =>
            {
                System.Diagnostics.Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Aoba"));
            });
        }

        private void NotifyScreenshotSaved(string path)
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

        private void NotifyVideoSaved(string path)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(path + " is saved."));

            var imagePath = "file:///" + Path.GetFullPath("Aoba.png");
            var imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            ToastNotification toast = new ToastNotification(toastXml);
            toast.Activated += (sender, args) => { System.Diagnostics.Process.Start("explorer", @"/select," + path); };
            toast.Dismissed += (sender, args) => { };
            toast.Failed += (sender, args) => { };

            const string APP_ID = "Daruyanagi.Aoba";

            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        }

        private void NotifyMessage(string message)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(message));

            var imagePath = "file:///" + Path.GetFullPath("Aoba.png");
            var imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            ToastNotification toast = new ToastNotification(toastXml);
            toast.Activated += (sender, args) => { System.Diagnostics.Process.Start("explorer", PictureStoragePath); };
            toast.Dismissed += (sender, args) => { };
            toast.Failed += (sender, args) => { };

            const string APP_ID = "Daruyanagi.Aoba";

            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        }

        private void CaptureGameArea(string path)
        {
            var bitmap = CaptureDesktop(SelectedDesktop);

            bitmap = bitmap.Clone(Rectangle, bitmap.PixelFormat);
            
            bitmap.Save(path);
        }

        private System.Windows.Media.Brush burstCaptureButtonBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);

        public System.Windows.Media.Brush BurstCaptureButtonBackgroundBrush
        {
            get { return burstCaptureButtonBackgroundBrush; }
            set { SetProperty(ref burstCaptureButtonBackgroundBrush, value); }
        }

        private System.Windows.Media.Brush videoCaptureButtonBackgroundBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);

        public System.Windows.Media.Brush VideoCaptureButtonBackgroundBrush
        {
            get { return videoCaptureButtonBackgroundBrush; }
            set { SetProperty(ref videoCaptureButtonBackgroundBrush, value); }
        }

        public int BurstCaptureInterval
        {
            get { return BurstTimer.Interval; }
            set
            {
                if (BurstTimer.Interval == value) return;

                BurstTimer.Interval = value;
                RaisePropertyChanged();
            }
        }

        private int frameRate = 10;

        public int FrameRate
        {
            get { return frameRate; }
            set { SetProperty(ref frameRate, value);  }
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
            set
            {
                SetProperty(ref selectedDesktop, value);

                CanCapture = false;
                Rectangle = Rectangle.Empty;
            }
        }

        public string PictureStoragePath
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

        public string VideoStoragePath
        {
            get
            {
                var path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

                path = Path.Combine(path, "Aoba");

                path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd"));

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
            }
        }

        public string GeneratePicturePath(string extension = "png")
        {
            var path = PictureStoragePath;

            path = System.IO.Path.Combine(path, DateTime.Now.ToString("HHmmss-ff"));
            path = System.IO.Path.ChangeExtension(path, extension);

            return path;
        }

        public string GenerateVideoPath(string extension = "avi")
        {
            var path = VideoStoragePath;

            path = System.IO.Path.Combine(path, DateTime.Now.ToString("HHmmss-ff"));
            path = System.IO.Path.ChangeExtension(path, extension);

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
                            if (w > h) return new Rectangle(x, y, w, h);
                        }
                    }
                }
            }

            throw new Exception("Game Area is not found.");
        }
    }
}
