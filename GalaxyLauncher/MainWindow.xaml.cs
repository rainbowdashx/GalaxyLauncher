using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;




namespace GalaxyLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        enum ELauncherMode
        {
            Install,
            Play,
            Repair
        }

        public static MainWindow WindowInstance;
        private ELauncherMode launcherMode = ELauncherMode.Install;
        private Process gameProcess;
        private Task<List<string>> FileCheckTask;
        private Task DownloaderTask;


        public MainWindow()
        {
            InitializeComponent();
            WindowInstance = this;
            InitLauncher();
        }

        private bool IsGameInstalled()
        {
            bool bGamefolder = Directory.Exists(Properties.Settings.Default.GamePath);
            bool bVersionFile = File.Exists(System.IO.Path.Combine(Properties.Settings.Default.GamePath, StreamHelper.gameVersionFilePathLocal));

            return bVersionFile && bGamefolder;
        }

        private void UpdateInstallPath()
        {
            lblInstallPath.Content = Properties.Settings.Default.GamePath;
        }

        private void InitLauncher()
        {
            UpdateInstallPath();

            if (!IsGameInstalled())
            {
                SetInstallMode();
            }
            else
            {
                if (CheckGameVersion())
                {
                    SetRepairMode();
                }
                SetPlayMode();
            }

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private bool CheckGameVersion()
        {
            string oldVersionString = "0.0.0";
            string newVersionString = StreamHelper.ReadFile(StreamHelper.gameVersionFilePathOnline);

            try
            {
                oldVersionString = File.ReadAllText(System.IO.Path.Combine(Properties.Settings.Default.GamePath, StreamHelper.gameVersionFilePathLocal));
            }
            catch (Exception ex)
            {
                oldVersionString = "0.0.0";
                Console.WriteLine(ex.Message);
            }

            Version newVersion = new Version(newVersionString);
            Version oldVersion = new Version(oldVersionString);


            return newVersion.CompareTo(oldVersion) > 0;
        }

        private async void DownloadGame()
        {

            if (GetWorkerStatus())
            {
                return;
            }

            ToggleInstallButtons(false);


            String data = StreamHelper.ReadFile(StreamHelper.manifestFileOnline);
            List<string> lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var progress = new Progress<int>(percent =>
            {
                pgbDownload.Dispatcher.BeginInvoke(new Action(delegate ()
                {

                    pgbDownload.Value = percent;
                }));
            });

            await pgbDownload.Dispatcher.BeginInvoke(new Action(delegate ()
           {
               pgbDownload.Value = 0;
               pgbDownload.Maximum = lines.Count;
               pgbDownload.Foreground = Brushes.Orange;
           }));
            FileCheckTask = Task.Run(() => StreamHelper.CheckGameFiles(lines, progress));
            lines = await FileCheckTask;


            await pgbDownload.Dispatcher.BeginInvoke(new Action(delegate ()
           {
               pgbDownload.Value = 0;
               pgbDownload.Maximum = lines.Count;
               pgbDownload.Foreground = Brushes.Green;
           }));
            DownloaderTask = StreamHelper.DownloadGameFiles(lines, progress);
            await DownloaderTask;

            ToggleInstallButtons(true);
            InitLauncher();

        }

        private void ToggleInstallButtons(bool enabled)
        {
            btnDownload.IsEnabled = enabled;
            btnPlay.IsEnabled = enabled;
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            DownloadGame();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {

            if (GetWorkerStatus())
            {
                return;
            }

            if (launcherMode == ELauncherMode.Install)
            {

                DownloadGame();

            }
            else if (launcherMode == ELauncherMode.Repair)
            {

                DownloadGame();
            }
            else if (launcherMode == ELauncherMode.Play)
            {


                gameProcess = new Process();
                gameProcess.StartInfo.FileName = System.IO.Path.Combine(Properties.Settings.Default.GamePath, StreamHelper.gameExecutable);
                gameProcess.EnableRaisingEvents = true;
                gameProcess.Exited += GameProcess_Exited;
                gameProcess.Start();

            }
        }

        private void GameProcess_Exited(object sender, EventArgs e)
        {
            if (gameProcess != null)
            {

                int code = gameProcess.ExitCode;
                if (code != 0)
                {
                    SetRepairMode();
                }
            }
        }

        public void SetFilenameLabelText(string text)
        {
            lblCurrentFile.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                lblCurrentFile.Content = text;
            }));
        }

        private void SetRepairMode()
        {
            btnPlay.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                btnPlay.Background = Brushes.OrangeRed;
                btnPlay.Content = "Update";
                launcherMode = ELauncherMode.Repair;
            }));
        }

        private void SetInstallMode()
        {
            btnPlay.Background = Brushes.Yellow;
            btnPlay.Content = "Install";
            launcherMode = ELauncherMode.Install;
        }
        private void SetPlayMode()
        {
            btnPlay.Background = Brushes.Green;
            btnPlay.Content = "Play";
            launcherMode = ELauncherMode.Play;
        }

        private bool GetWorkerStatus()
        {
            return GetTaskStatus(DownloaderTask) || GetTaskStatus(FileCheckTask);
        }


        private bool GetTaskStatus(Task task)
        {
            return (DownloaderTask != null) && (DownloaderTask.IsCompleted == false ||
                           DownloaderTask.Status == TaskStatus.Running ||
                           DownloaderTask.Status == TaskStatus.WaitingToRun ||
                           DownloaderTask.Status == TaskStatus.WaitingForActivation);
        }

        private void btnInstallPath_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Properties.Settings.Default.GamePath = fbd.SelectedPath + "\\GalaxyGame\\";
                    Properties.Settings.Default.Save();
                    UpdateInstallPath();
                }
            }
        }
    }
}
