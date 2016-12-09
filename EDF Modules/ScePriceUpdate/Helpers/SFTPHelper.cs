    #region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Renci.SshNet;
using WheelsScraper;

#endregion

namespace ScePriceUpdate.Helpers
{
    public static class SFTPHelper
    {
        public static List<string> LoadFilesList(ScraperSettings settings, int port, string priceFolder)
        {
            List<string> files;

            var ftp = settings.FtpAddress.TrimEnd('/');
            if (ftp.ToLower().StartsWith("sftp"))
            {
                ftp = ftp.Replace("sftp://", "").Trim('/');
            }

            using (var sftp = new SftpClient(ftp, port, settings.CustomFtpUsername, settings.CustomFtpPassword))
            {
                sftp.Connect();
                files =
                    sftp.ListDirectory(priceFolder).Where(f => !f.Name.StartsWith(".")).Select(f => f.Name).ToList();

                sftp.Disconnect();
            }
            return files;
        }

        public static string DownloadFile(ScraperSettings settings, int port, string priceFolder, string fileName)
        {
            var date = string.Format("_{0:dd-MM-yyyy_HH-mm}_", DateTime.Now);
            var filePath = Path.Combine(Path.GetTempPath(), date + fileName);

            var ftp = settings.FtpAddress.TrimEnd('/');
            if (ftp.ToLower().StartsWith("sftp"))
            {
                ftp = ftp.Replace("sftp://", "").Trim('/');
            }
            using (var sftp = new SftpClient(ftp, port, settings.CustomFtpUsername, settings.CustomFtpPassword))
            {
                sftp.Connect();

                sftp.ChangeDirectory(priceFolder);

                using (var fs = File.OpenWrite(filePath))
                {
                    sftp.DownloadFile(fileName, fs);
                }
                sftp.Disconnect();
            }
            return filePath;
        }
    }
}
