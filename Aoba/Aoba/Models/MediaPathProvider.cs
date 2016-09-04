using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoba.Models
{
    public static class MediaPathProvider
    {
        private static List<string> history = new List<string>();

        public static string GetVideoFolder()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            path = Path.Combine(path, "Aoba");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static string GetPictureFolder()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            path = Path.Combine(path, "Aoba");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static List<string> GetHistory()
        {
            // 履歴を呼び出すタイミングで、存在しないファイルをはじいておく
            history = history.Where(_ => File.Exists(_)).OrderByDescending(_ => _).ToList();

            return history;
        }

        public static string Generate(MediaType media_type)
        {
            var path = string.Empty;
            var extension = media_type.ToString().ToLower();

            switch (media_type)
            {
                case MediaType.Avi:
                case MediaType.Mp4:
                    path = GetVideoFolder();
                    break;
                case MediaType.Png:
                case MediaType.Gif:
                    path = GetPictureFolder();
                    break;
            }
            
            path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.Combine(path, DateTime.Now.ToString("HHmmssff"));
            path = Path.ChangeExtension(path, extension);

            // 履歴は Twitter のアップデートで使おうと思ってるんだけど、
            // 今のところは画像のみ対応にしておく
            if (media_type == MediaType.Png) history.Add(path);

            return path;
        }
    }
}
