using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyLauncher
{
    public class Game
    {
        public String gameVersionFilePathOnline;
        public String gameVersionFilePathLocal;
        public String manifestFileOnline;
        public String gameExecutable;
        public String gameFilesPathOnline;


        public String GameName;

        public String GameFoldername;


        public Game(String gameVersionOnline, String GameVersionLocal, String ManifestOnline, String GameExe, String GameFilesOnline, String inGameName, String FolderName)
        {
            gameVersionFilePathOnline = gameVersionOnline;
            gameVersionFilePathLocal = GameVersionLocal;
            manifestFileOnline = ManifestOnline;
            gameExecutable = GameExe;
            gameFilesPathOnline = GameFilesOnline;
            GameName = inGameName;
            GameFoldername = FolderName;
        }
    }
}
