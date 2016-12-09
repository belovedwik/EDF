using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WheelsScraper;
using Renci.SshNet;
using System.IO;
using Databox.Libs.Sanmar;
using System.Globalization;

namespace Sanmar.Helper
{
    public static class SFTPHelper
    {
        public static List<string> LoadFilesList(ScraperSettings settings, ExtSettings extSet)
        {
            List<string> files;
           
            string folder = (!string.IsNullOrEmpty(extSet.FTPWorkingDirectory)) ? extSet.FTPWorkingDirectory : "";

            int port;
            string ftp = string.Empty;

            GetFTPConfig(settings, extSet, out ftp, out port);
           
            using (var sftp = new SftpClient(ftp, port, settings.CustomFtpUsername, settings.CustomFtpPassword))
            {
                sftp.Connect();
                files = sftp.ListDirectory(folder).Where(f => !f.Name.StartsWith(".")).Select(f => f.Name).ToList();
                sftp.Disconnect();
            }

            return files.FilterByDateRange(extSet);
        }

        private static void GetFTPConfig(ScraperSettings settings, ExtSettings extSett, out string ftp, out int port) {
            
            port = extSett.FTPPort > 0 ? extSett.FTPPort : 21;
            ftp = settings.FtpAddress.TrimEnd('/');
            if (ftp.ToLower().StartsWith("sftp"))
            {
                ftp = ftp.Replace("sftp://", "").Trim('/');
                port = extSett.FTPPort > 0 ? extSett.FTPPort : 2200;
            }
        }

        public static List<string> FilterByDateRange(this List<string> files, ExtSettings extSet)
        {
            List<string> filteredFiles = new List<string>();
            if (extSet.DateFrom > DateTime.MinValue && extSet.DateTo > DateTime.MinValue) {
                try
                {
                    foreach (string file in files) {
                        DateTime curFileDT = DateTime.ParseExact(file.Replace("status.txt", ""), "MM-dd-yy", CultureInfo.InvariantCulture);
                        if (curFileDT >= extSet.DateFrom.Date && curFileDT <= extSet.DateTo.Date)
                            filteredFiles.Add(file);
                    }
                    files = filteredFiles;
                }
                catch { }

                return files;
            }
            else
                return files;
        }

        public static string DownloadFile(ScraperSettings settings, ExtSettings extSett, string fileName)
        {
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            int port;
            string ftp = string.Empty;

            GetFTPConfig(settings, extSett, out ftp, out port);

            using (var sftp = new SftpClient(ftp, extSett.FTPPort, settings.CustomFtpUsername, settings.CustomFtpPassword))
            {
                sftp.Connect();

                sftp.ChangeDirectory(extSett.FTPWorkingDirectory);

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
