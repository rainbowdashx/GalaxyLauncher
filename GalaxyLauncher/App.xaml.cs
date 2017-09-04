using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;


namespace GalaxyLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);




            using (var mgr = new UpdateManager("http://h2412564.stratoserver.net"))
            {
                await mgr.UpdateApp();

            }

        }
    }
}
