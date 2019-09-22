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

        public static Uri host = new Uri("ftp://h2844272.stratoserver.net/");
        //         public static String gameVersionFilePathOnline = "/game/Win64/bin/GameVersion.txt";
        //         public static String gameVersionFilePathLocal = "game/GameVersion.txt";
        //         public static String manifestFileOnline = "/game/Win64/GameManifest.txt";
        //         public static String gameExecutable = "game/dummyWoW.exe";
        //         public static string gameFilesPathOnline = "game/Win64/bin/";




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




        public static async Task DownloadFile(string path, long filesize)
        {
#if DEBUG


            await MainWindow.WindowInstance.pgbFileProgress.Dispatcher.BeginInvoke(new Action(delegate ()
             {
                 MainWindow.WindowInstance.pgbFileProgress.Value = filesize / 2;
                 MainWindow.WindowInstance.pgbFileProgress.Maximum = filesize;
             }));


            MainWindow.WindowInstance.SetFilenameLabelText(GetLocalFilePath(path));

            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });


#else
            if (MainWindow.CurrentGame != null)
            {


                MainWindow.WindowInstance.SetFilenameLabelText(GetLocalFilePath(path));

                await MainWindow.WindowInstance.pgbFileProgress.Dispatcher.BeginInvoke(new Action(delegate ()
                 {
                     MainWindow.WindowInstance.pgbFileProgress.Value = 0;
                     MainWindow.WindowInstance.pgbFileProgress.Maximum = filesize;
                 }));

                string filePathSave = GetLocalFilePath(path);

                EnsureFolder(filePathSave);

                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnFileDownloadProgress);
                    await client.DownloadFileTaskAsync(new Uri(host, MainWindow.CurrentGame.gameFilesPathOnline + path), filePathSave);

                }
            }
#endif
        }



        public static Task<List<String>> CheckGameFiles(List<String> fileList, IProgress<int> progress)
        {
            int FileNum = 0;

            List<String> CheckFilesForDeletion = new List<string>();
            CheckFilesForDeletion.AddRange(new string[] { "\\galaxy\\Content\\Paks\\pakchunk0-WindowsNoEditor_P.pak",
                                                     "\\galaxy\\Content\\Paks\\dummyWoW-WindowsNoEditor.pak",
                                                     "\\galaxy\\Content\\Paks\\galaxy-WindowsNoEditor.pak"

            });

            for (int i = 0; i < 6; i++)
            {
                CheckFilesForDeletion.Add(String.Format("\\galaxy\\Content\\Paks\\pakchunk{0}-WindowsNoEditor_0_P.pak", i));
            }
            for (int i = 0; i < 6; i++)
            {
                CheckFilesForDeletion.Add(String.Format("\\galaxy\\Content\\Paks\\galaxy-WindowsNoEditor_{0}_P.pak", i));
            }


            foreach (var item in fileList)
            {
                string[] fileString = item.Split(':');
                string fileName = fileString[0];
                string fileHash = fileString[1];
                string fileSize = fileString[2];

                string filePath = GetLocalFilePath(fileName);


                CheckFilesForDeletion.Remove(fileName);

            
                List<String> SkipFileList = new List<string>();
                if (File.Exists(filePath) && GetChecksum(filePath).Equals(fileHash))
                {
                    SkipFileList.Add(item);
                }
                fileList = fileList.Except(SkipFileList).ToList();
                FileNum++;
                if (progress != null)
                {
                    progress.Report(FileNum);
                }
            }


            foreach (var item in CheckFilesForDeletion)
            {
                string LocalFilePath = GetLocalFilePath(item);
                if (File.Exists(LocalFilePath))
                {
                    File.Delete(LocalFilePath);
                }
            }

            return Task.FromResult(fileList);
        }

        public static async Task DownloadGameFiles(List<String> fileList, IProgress<int> progress)
        {

            int FileNum = 0;
            foreach (var item in fileList)
            {
                string[] fileString = item.Split(':');
                string fileName = fileString[0];
                string fileHash = fileString[1];
                string fileSize = fileString[2];
                Console.WriteLine(fileName);
                await DownloadFile(fileName, long.Parse(fileSize));
                FileNum++;
                if (progress != null)
                {
                    progress.Report(FileNum);
                }
            }
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

        private static long GetFileSize(string path)
        {
            //File Size Query
            if (MainWindow.CurrentGame != null)
            {
                FtpWebRequest sizeRequest = (FtpWebRequest)WebRequest.Create(MainWindow.CurrentGame.gameFilesPathOnline + path);
                sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                using (WebResponse response = sizeRequest.GetResponse())
                {
                    return response.ContentLength;
                }
            }
            return 0;
        }

        private static void OnFileDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            MainWindow.WindowInstance.pgbFileProgress.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                MainWindow.WindowInstance.pgbFileProgress.Value = e.BytesReceived;
            }));
        }
        private static string GetLocalFilePath(string filename)
        {
            return string.Format("{0}{2}{1}", Properties.Settings.Default.GamePath, filename, MainWindow.CurrentGame.GameFoldername);
        }
    }
}
