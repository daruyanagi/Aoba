using CoreTweet;
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
using Media = System.Windows.Media;

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
        public ICommand TwitterAuthorizeCommand { get; private set; }
        public ICommand TwitterPostCommand { get; private set; }

        public MainWindowViewModel()
        {
            CanTwitter = Models.Twitter.GetInstance().Initialize();
            if (Properties.Settings.Default.Notify)
                Models.NotificationProvider.Enable();
            else
                Models.NotificationProvider.Disable();
            SelectedDesktop = Properties.Settings.Default.SelectedDesktop;

            DetectCommand = new DelegateCommand(_ =>
            {
                var game_area = Models.GameArea.GetInstance();

                game_area.Detect(game_area.Screen);

                RaisePropertyChanged("CanCapture");
            });

            SingleCaptureCommand = new DelegateCommand(_ =>
            {
                var engine = Models.SingleImageCaptureEngine.GetInstance();

                engine.Save();
            },
            _ => CanCapture);

            BurstCaptureCommand = new DelegateCommand(_ =>
            {
                var engine = Models.BurstImageCaptureEngine.GetInstance();

                if (!engine.Recording)
                {
                    engine.Start();
                    BurstCaptureButtonBackgroundBrush = new Media.SolidColorBrush(Media.Colors.Orange);
                }
                else
                {
                    engine.Stop();
                    BurstCaptureButtonBackgroundBrush = new Media.SolidColorBrush(Media.Colors.Black);
                }
            },
            _ => CanCapture);

            VideoCaptureCommand = new DelegateCommand(_ =>
            {
                var engine = Models.VideoCaptureEngine.GetInstance();

                if (!engine.Recording)
                {
                    engine.Start();
                    VideoCaptureButtonBackgroundBrush = Media.Brushes.Orange;
                }
                else
                {
                    engine.Stop();
                    VideoCaptureButtonBackgroundBrush = Media.Brushes.Black;
                }

            },
            _ => CanCapture && Screen.AllScreens[SelectedDesktop].Primary);

            OpenPictureFolderCommand = new DelegateCommand(_ =>
            {
                Process.Start(Models.MediaPathProvider.GetPictureFolder());
            });

            OpenVideoFolderCommand = new DelegateCommand(_ =>
            {
                Process.Start(Models.MediaPathProvider.GetVideoFolder());
            });

            TwitterAuthorizeCommand = new DelegateCommand(_ =>
            {
                var window = new Views.TwitterAuthWindow();

                window.ShowDialog();

                CanTwitter = Models.Twitter.GetInstance().IsEnabed;
            });

            TwitterPostCommand = new DelegateCommand(_ =>
            {
                Models.SingleImageCaptureEngine.GetInstance().Save();
                var window = new Views.TwitterPostWindow();
                window.ShowDialog();
            },
            _ => CanTwitter && CanCapture /* && Models.MediaPathProvider.GetHistory().Count > 0 */);
        }

        public string Title
        {
            get { return $"Aoba {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"; }
        }

        private Media.Brush burstCaptureButtonBackgroundBrush = new Media.SolidColorBrush(Media.Colors.Black);

        public Media.Brush BurstCaptureButtonBackgroundBrush
        {
            get { return burstCaptureButtonBackgroundBrush; }
            set { SetProperty(ref burstCaptureButtonBackgroundBrush, value); }
        }

        private Media.Brush videoCaptureButtonBackgroundBrush = new Media.SolidColorBrush(Media.Colors.Black);

        public Media.Brush VideoCaptureButtonBackgroundBrush
        {
            get { return videoCaptureButtonBackgroundBrush; }
            set { SetProperty(ref videoCaptureButtonBackgroundBrush, value); }
        }

        public int BurstCaptureInterval
        {
            get
            {
                var engine = Models.BurstImageCaptureEngine.GetInstance();
                return engine.Interval;
            }
            set
            {
                var engine = Models.BurstImageCaptureEngine.GetInstance();

                if (engine.Interval == value) return;

                engine.Interval = value;
                RaisePropertyChanged();
            }
        }

        public int FrameRate
        {
            get
            {
                var engine = Models.VideoCaptureEngine.GetInstance();
                return engine.FrameRate;
            }
            set
            {
                var engine = Models.VideoCaptureEngine.GetInstance();

                if (engine.FrameRate == value) return;

                engine.FrameRate = value;
                RaisePropertyChanged();
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                var game_area = Models.GameArea.GetInstance();

                return game_area.Rects == null || game_area.Rects.Length == 0
                    ? Rectangle.Empty
                    : game_area.Rects.First();
            }
        }

        public bool CanCapture { get { return !Rectangle.IsEmpty; } }

        private bool canTwitter = false;

        public bool CanTwitter
        {
            get { return canTwitter; }
            set { SetProperty(ref canTwitter, value); }
        }
        
        public bool IsNotify
        {
            get { return Models.NotificationProvider.IsEnabled(); }
            set
            {
                if (value == Models.NotificationProvider.IsEnabled()) return;

                if (value)
                    Models.NotificationProvider.Enable();
                else
                    Models.NotificationProvider.Disable();

                RaisePropertyChanged();
            }
        }

        public Screen[] Desktops
        {
            get { return Screen.AllScreens; }
        }

        public Models.MediaType MediaType
        {
            get { return Models.VideoCaptureEngine.GetInstance().MediaType; }
            set { Models.VideoCaptureEngine.GetInstance().MediaType = value; }
        }

        public Models.MediaType[] MediaTypes
        {
            get { return new Models.MediaType[] { Models.MediaType.Avi, Models.MediaType.Gif, }; }
        }

        private int selectedDesktop = -1;

        public int SelectedDesktop
        {
            get { return selectedDesktop; }
            set
            {
                if (selectedDesktop == value) return;

                SetProperty(ref selectedDesktop, value);

                var game_area = Models.GameArea.GetInstance().Screen = Screen.AllScreens[selectedDesktop];
                Properties.Settings.Default.SelectedDesktop = value;

                RaisePropertyChanged("Rectangle");
                RaisePropertyChanged("CanCapture");
            }
        }
    }
}
