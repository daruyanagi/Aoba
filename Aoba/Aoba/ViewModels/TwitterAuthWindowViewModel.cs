using CoreTweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static CoreTweet.OAuth;

namespace Aoba.ViewModels
{
    class TwitterAuthWindowViewModel : ViewModelBase
    {
        public ICommand GetPinCodeCommand { get; private set; }
        public ICommand GetTokensCommand { get; private set; }

        private Models.Twitter model = Models.Twitter.GetInstance();
        // private Models.NotifyProviderModel notify = Models.NotifyProviderModel.GetInstance();

        public TwitterAuthWindowViewModel()
        {
            GetPinCodeCommand = new DelegateCommand(_ =>
            {
                model.GetPinCode();
            });

            GetTokensCommand = new DelegateCommand(_ =>
            {
                var result = model.GetTokens(PinCode);
            });
        }

        private string pinCode = string.Empty;

        public string PinCode
        {
            get { return pinCode; }
            set { SetProperty(ref pinCode, value); }
        }
    }
}
