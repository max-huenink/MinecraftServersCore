using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace MinecraftServersCore
{
    public partial class Server
    {
        /// <summary>
        /// The path to the world folder that is backed up/restored
        /// </summary>
        private string WorldDir => Path.Combine(ServerDir, ServerName);
        public void Start()
        {
            // Makes sure the current directory is the server's directory
            if (Directory.GetCurrentDirectory() != ServerDir)
            {
                Directory.SetCurrentDirectory(ServerDir);
            }

            switch (LaunchType.ToLower().First<char>()) // Decides what to do when starting the server
            {
                case 'b': // backup
                    Backup();
                    break;
                case 'r': // restore
                    Restore();
                    break;
                case 'l': // legacy
                    if (Version == "")
                    {
                        // Updates the server's version with the new version
                        ChangeVersion(AskForVersion()); // If legacy is specified, the version is requested (if not specified)
                    }
                    // If the version has not been specified, ask. Otherwise assume the specified version is valid and continue
                    break;
                default: // default/normal start
                    // Everything that needs to happen happened in Launch
                    break;
            }

            // Check if the newest version is downloaded
            Update(ServerType);

            if (File.Exists(LaunchJar))
            {
                Process exec = new Process()
                {
                    EnableRaisingEvents = false,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c java -Xmx2G -jar \"{LaunchJar}\" nogui",
                        UseShellExecute = false,
                    },
                };
                Console.Clear();
                Console.Title = $"Minecraft Server: {ServerName}. Version: {Version}";

                exec.Start();
            }
            else
            {
                Console.WriteLine("Launch file doesn't exist, would you like to download it?");
                Console.Write("Download? Y/[N]: ");
                bool retry = Funcs.ValidateBool(false);
                if (retry)
                {
                    Update(ServerType, Version);
                    Start();
                }
                // Else, close
            }
        }

        /// <summary>
        /// Backs the server up, then resumes starting
        /// </summary>
        /// <param name="modifier">A modifier to add to the end of the backup filename</param>
        private void Backup(string modifier = "")
        {
            // Current dateTime
            DateTime date = DateTime.Now;
            // Adds text in front of the modifier variable if it is nonempty
            if (!string.IsNullOrEmpty(modifier))
            {
                modifier = $".{modifier}";
            }
            // The name for the backup
            string backupName = ServerName +
                $"_{date.Year}-{date.Month}-{date.Day}-{date.Hour}.{date.Minute}.{date.Second}{modifier}.zip";

            // The source directory path
            //string sourceDir = Path.Combine(Server.ServerDir, Server.ServerName);
            // The destination zip file path
            string destFile = Path.Combine(ServerDir, backupName);

            // Checks that the directory being backed up exists, and exits if it doesn't.
            if (!Directory.Exists(WorldDir))
            {
                Console.WriteLine("Source file does not exist.");
                Console.ReadLine();
                return;
            }

            // Zips the source directory to the source file
            ZipFile.CreateFromDirectory(WorldDir, destFile);

            Console.WriteLine($"{ServerName} backed up to {backupName}");
            Console.ReadLine();
        }

        /// <summary>
        /// Restores the server from a previous backup, then resumes starting
        /// </summary>
        private void Restore()
        {
            Console.Clear();
            string[] allFiles = Directory.GetFiles(ServerDir);
            Dictionary<int, string> backups = new Dictionary<int, string>();
            int fileCount = -1;
            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) == ".zip")
                {
                    fileCount++;
                    backups.Add(fileCount, file);
                }
            }
            Console.WriteLine("Enter the number for the backup you want to restore with");
            // PrettyPrints each backup file without path or extension preceded by its key in the dictionary `files`
            Funcs.PrettyPrint(backups.Select(i => i.Key + " " + Path.GetFileNameWithoutExtension(i.Value)).ToList<string>());
            Console.Write($"Backup [{fileCount}]: ");
            // The backup to use
            int backup = Funcs.GetInt(0, fileCount, fileCount);

            // Only attempt to backup and delete the source directory if it exists
            if (Directory.Exists(WorldDir))
            {
                Backup("pre-restore");
                Directory.Delete(WorldDir, true);
                Console.WriteLine($"Deleted {ServerName}");
            }

            // Extracts specified backup to the source directory
            ZipFile.ExtractToDirectory(backups[backup], ServerDir);
            Console.WriteLine($"Extracted {Path.GetFileNameWithoutExtension(backups[backup])}");
            Console.WriteLine($"{ServerName} restored from {Path.GetFileNameWithoutExtension(backups[backup])}");
            Backup("post-restore");
        }

        /// <summary>
        /// Ask for the version to be used
        /// </summary>
        /// <returns>A validated version string</returns>
        private string AskForVersion()
        {
            Console.Clear();
            Console.WriteLine("Available versions:");
            string[] files = Directory.GetFiles(Variables.vFiles);
            List<string> versions = new List<string>();
            foreach (string file in files)
            {
                versions.Add(Path.GetFileNameWithoutExtension(file).Replace("minecraft_server.", ""));
            }
            Funcs.PrettyPrint(versions);
            Console.WriteLine($"Version [{Variables.VV}]: ");
            return Funcs.NullEmpty(Variables.VV);
        }
    }
}
