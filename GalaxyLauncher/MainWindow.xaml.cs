using System;
using System.Collections.Generic;
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


        public static MainWindow WindowInstance;
        private bool RepairMode = false;



        public MainWindow()
        {
            InitializeComponent();
            WindowInstance = this;
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



        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {


            btnDownload.IsEnabled = false;


            String oldVersionString = "0.0.0";
            String newVersionString = StreamHelper.ReadFile(StreamHelper.gameVersionFilePathOnline);

            try
            {
                oldVersionString = File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, StreamHelper.gameVersionFilePathLocal));
            }
            catch (Exception ex)
            {
                oldVersionString = "0.0.0";
                Console.WriteLine(ex.Message);
            }
            if (RepairMode)
            {
                oldVersionString = "0.0.0";
                RepairMode = false;
            }

            Version newVersion = new Version(newVersionString);
            Version oldVersion = new Version(oldVersionString);


            int isNewer = newVersion.CompareTo(oldVersion);



            if (isNewer > 0)
            {
                String data = StreamHelper.ReadFile(StreamHelper.manifestFileOnline);
                List<string> lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();





                var progress = new Progress<int>(percent =>
                {
                    pgbDownload.Value = percent;
                });

                pgbDownload.Value = 0;
                pgbDownload.Maximum = lines.Count;
                pgbDownload.Foreground = Brushes.Orange;
                lines = await Task.Run(() => StreamHelper.CheckGameFiles(lines, progress));

                pgbDownload.Value = 0;
                pgbDownload.Maximum = lines.Count;
                pgbDownload.Foreground = Brushes.Green;
                await Task.Run(() => StreamHelper.DownloadGameFiles(lines, progress));


            }

            btnDownload.IsEnabled = true;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = System.IO.Path.Combine(Environment.CurrentDirectory, StreamHelper.gameExecutable);
            process.EnableRaisingEvents = true;
            process.Start();

            process.Exited += (sender_, e_) =>
            {
                int code = process.ExitCode;
                if (code != 0)
                {
                    RepairMode = true;
                }
                process.Dispose();
            };



        }

        public void SetFilenameLabelText(string text)
        {
            lblCurrentFile.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                lblCurrentFile.Content = text;
            }));
        }

    }
}
