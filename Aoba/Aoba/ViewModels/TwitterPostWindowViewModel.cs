using CoreTweet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoba.ViewModels
{
    class TwitterPostWindowViewModel : ViewModelBase
    {
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand TwitterPostCommand { get; private set; }

        private Models.TwitterProviderModel model = Models.TwitterProviderModel.GetInstance();

        public TwitterPostWindowViewModel(string path) : base()
        {
            Path = path;

            CloseCommand = new DelegateCommand(_ =>
            {
                var window = _ as System.Windows.Window;

                window.Close();
            });

            TwitterPostCommand = new DelegateCommand(_ =>
            {
                var result = model.PostWithMedia(Message, new string[] { Path, });

                if (result)
                {
                    var window = _ as System.Windows.Window;

                    window.Close();
                }
            });
        }

        private string path = string.Empty;

        public string Path
        {
            get { return path; }
            set { SetProperty(ref path, value); }
        }

        private string message = string.Empty;

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }
    }
}
