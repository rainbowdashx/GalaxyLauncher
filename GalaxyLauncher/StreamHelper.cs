using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyLauncher
{
    public static class StreamHelper
    {

        public static Uri host = new Uri("ftp://h2412564.stratoserver.net/");
        public static String gameVersionFilePathOnline = "/game/Win64/bin/GameVersion.txt";
        public static String gameVersionFilePathLocal = "game/GameVersion.txt";
        public static String manifestFileOnline = "/game/Win64/GameManifest.txt";
        public static String gameExecutable = "game/dummyWoW.exe";

        public static String ReadFile(String path)
        {
            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(host, path));

            FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string data = reader.ReadToEnd();
            Console.WriteLine(data);

            response.Close();
            responseStream.Close();

            return data;

        }


        public static void DownloadFile(String path)
        {
#if DEBUG

            System.Threading.Thread.Sleep(1000);


#else
            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(host, "game/Win64/bin/" + path));

            String filePath = string.Format("{0}game{1}", AppDomain.CurrentDomain.BaseDirectory, path);
            EnsureFolder(filePath);

            using (Stream ftpStream = ftp.GetResponse().GetResponseStream())
            using (Stream fileStream = File.Create(filePath))
            {
                ftpStream.CopyTo(fileStream);
            }
#endif
        }

        public static Task<List<String>> CheckGameFiles(List<String> fileList, IProgress<int> progress)
        {

            int FileNum = 0;
            foreach (var item in fileList)
            {
                String[] fileString = item.Split(':');
                String fileName = fileString[0];
                String fileHash = fileString[1];
                String fileSize = fileString[2];

                String filePath = string.Format("{0}game{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);

                List<String> removeList = new List<string>();
                if (File.Exists(filePath) && GetChecksum(filePath).Equals(fileHash))
                {
                    removeList.Add(item);
                }
                fileList = fileList.Except(removeList).ToList();
                FileNum++;
                if (progress != null)
                {
                    progress.Report(FileNum);
                }
            }
            return Task.FromResult(fileList);
        }

        public static Task DownloadGameFiles(List<String> fileList, IProgress<int> progress)
        {

            int FileNum = 0;
            foreach (var item in fileList)
            {
                String[] fileString = item.Split(':');
                String fileName = fileString[0];
                String fileHash = fileString[1];
                String fileSize = fileString[2];
                Console.WriteLine(fileName);
                DownloadFile(fileName);
                FileNum++;
                if (progress != null)
                {
                    progress.Report(FileNum);
                }
            }
            return Task.FromResult(0);
        }


        private static void EnsureFolder(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            {
                Directory.CreateDirectory(directoryName);
                Console.WriteLine("Folder Created");
            }
        }
        private static String GetChecksum(String path)
        {

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {

                    String zero = String.Empty;
                    String hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", zero);
                    return hash;
                }
            }
        }
    }
}
