using CoreTweet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CoreTweet.OAuth;

namespace Aoba.Models
{
    public sealed class Twitter
    {
        private static Twitter instance = new Twitter();

        public static Twitter GetInstance()
        {
            return instance;
        }

        private Twitter()
        {

        }

        Tokens tokens = null;
        OAuthSession session = null;

        public bool Initialize()
        {
            if (IsEnabed) return true;

            try
            {
                tokens = Tokens.Create(
                    Properties.Settings.Default.ConsumerKey,
                    Properties.Settings.Default.ConsumerSecret,
                    Properties.Settings.Default.AccessToken,
                    Properties.Settings.Default.AccessTokenSecret
                );

                var temp = tokens.Account.VerifyCredentials();

                return true;
            }
            catch (Exception e) // ネットワークエラーのときもクリアされちゃうの、どうしよっかな
            {
                tokens = null;

                return false;
            }
        }

        public void Expire()
        {
            Properties.Settings.Default.AccessToken = string.Empty;
            Properties.Settings.Default.AccessTokenSecret = string.Empty;
            Properties.Settings.Default.Save();
            
            tokens = null;
        }

        public bool GetPinCode()
        {
            try
            {
                session = OAuth.Authorize(
                    Properties.Settings.Default.ConsumerKey,
                    Properties.Settings.Default.ConsumerSecret
                );

                Process.Start(session.AuthorizeUri.ToString());

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GetTokens(string pin_code)
        {
            try
            {
                tokens = session.GetTokens(pin_code);

                Properties.Settings.Default.AccessToken = tokens.AccessToken;
                Properties.Settings.Default.AccessTokenSecret = tokens.AccessTokenSecret;
                Properties.Settings.Default.Save();

                NotificationProvider.Toast("Authorization has completed successfully.", "Aoba.png");

                return true;
            }
            catch
            {
                tokens = null;

                NotificationProvider.Toast("Authorization failed.", "Aoba.png");

                return false;
            }
        }

        public bool PostWithMedia(string message, string[] path)
        {
            try
            {
                if (message.Length > 140) throw new ArgumentException("message is too long");
                if (path.Length > 4) throw new ArgumentException("path must be <= 4");

                // とりあえず画像のみ

                var media = path
                    .Select(_ => tokens.Media.Upload(media: new FileInfo(_)).MediaId)
                    .ToArray();
                
                var status = tokens.Statuses.Update(status: message, media_ids: media);

                NotificationProvider.Toast("Tweet is posted.", path.First(), (s, e) => 
                {
                    Process.Start($"https://twitter.com/{status.User.ScreenName}/status/{status.Id}");
                });

                return true;
            }
            catch (Exception e)
            {
                tokens = null;

                NotificationProvider.Toast(e.Message, "Aoba.png");

                return false;
            }
        }

        public bool IsEnabed
        {
            get { return tokens != null; }
        }
    }
}
