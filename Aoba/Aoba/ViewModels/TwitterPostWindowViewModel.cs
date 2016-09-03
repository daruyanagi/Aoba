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
        public DelegateCommand BackCommand { get; private set; }
        public DelegateCommand NextCommand { get; private set; }
        public DelegateCommand TwitterPostCommand { get; private set; }

        private Models.Twitter model = Models.Twitter.GetInstance();

        public TwitterPostWindowViewModel()
        {
            Pictures = Models.MediaPathProvider.GetHistory();

            CloseCommand = new DelegateCommand(_ =>
            {
                var window = _ as System.Windows.Window;

                window.Close();
            });

            BackCommand = new DelegateCommand(_ => Index++, _ => Index < Pictures.Count - 1);

            NextCommand = new DelegateCommand(_ => Index--, _ => 0 < Index);

            TwitterPostCommand = new DelegateCommand(_ =>
            {
                var result = model.PostWithMedia(Message, new string[] { SelectedPicture, });

                if (result)
                {
                    var window = _ as System.Windows.Window;

                    window.Close();
                }
            });
        }

        private string selectedPicture = string.Empty;

        public string SelectedPicture
        {
            get { return Pictures[Index]; }
        }

        private int index = 0;

        public int Index
        {
            get { return index; }
            set { SetProperty(ref index, value); RaisePropertyChanged("SelectedPicture"); }
        }

        private List<string> pictures = null;

        public List<string> Pictures
        {
            get { return pictures; }
            set { SetProperty(ref pictures, value); }
        }

        private string message = string.Empty;

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }
    }
}
