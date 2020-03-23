using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using MinecraftServersCore.Data;

namespace MinecraftServersCore
{
    public partial class Server
    {
        private string NewVersion;

        // The launcher path
        private string LauncherPath => Path.Combine(Variables.vFiles, $"minecraft_server.{NewVersion}.jar");

        // Parses (and gets) the version manifest from json into C# objects
        private static VersionManifest Manifest = JsonConvert.DeserializeObject<VersionManifest>(parseJSON("https://launchermeta.mojang.com/mc/game/version_manifest.json"));

        /// <summary>
        /// Updates servers to specified version
        /// </summary>
        /// <param name="serverType">The type of server Vanilla/Snapshot/Legacy</param>
        /// <param name="newVersion">The new version to update to</param>
        /// <param name="changeVersionsTXT">Should the version be updated in the text file (if no, serverType is irrelevant)</param>
        public void Update(string serverType, string newVersion)
        {
            ServerType = serverType;
            NewVersion = newVersion;

            if (ServerType == "") // Gets serverType
            {
                Console.Clear();
                Console.WriteLine("What is the server type?");
                Console.WriteLine("Server types include:");
                Funcs.PrettyPrint(new string[] { "vanilla", "snapshot", "both" });
                Console.Write("Server Type [vanilla]: ");
                ServerType = Funcs.NullEmpty("vanilla").ToLower();
            }

            // Gets the new version if empty
            if (String.IsNullOrEmpty(NewVersion))
            {
                string currentVersion = "";
                string latestVersion = "";
                switch (ServerType.ToLower().First<char>())
                {
                    case 'v': // Vanilla
                        currentVersion = Variables.VV;
                        latestVersion = Manifest.latest.release;
                        Console.WriteLine($"Latest vanilla version is: {latestVersion}");
                        goto default;
                    case 's': // Snapshot
                        currentVersion = Variables.SV;
                        latestVersion = Manifest.latest.snapshot;
                        Console.WriteLine($"Latest snapshot version is: {latestVersion}");
                        goto default;
                    case 'b':
                        Update("both");
                        return;
                    default:
                        Console.WriteLine($"What is the new minecraft version? Current version is: {currentVersion}");
                        Console.Write($"New version [{latestVersion}]: ");
                        NewVersion = Funcs.NullEmpty(latestVersion);
                        break;
                }
                // The lower case `newVersion` is used because we want to know if the passed arg was null
                if (string.IsNullOrEmpty(currentVersion) || newVersion == null)
                {
                    UpdateVersions();
                    Variables.LoadVariables();
                }
            }

            // If the file already exists, don't do anything
            if (File.Exists(LauncherPath))
            {
                Console.WriteLine("File already exists.");
                UpdateVersions();
                Console.ReadLine();
                return;
            }

            Download();
        }

        /// <summary>
        /// Checks if there is a new version available (for vanilla and snapshot)
        /// </summary>
        public void Update(string serverType)
        {
            ServerType = serverType;
            if (string.IsNullOrEmpty(ServerType))
            {
                ServerType = "both";
            }
            string currentVersion = "";
            switch (ServerType.ToLower().First<char>())
            {
                case 'v': // Vanilla
                    currentVersion = Variables.VV;
                    NewVersion = Manifest.latest.release;
                    break;
                case 's': // Snapshot
                    currentVersion = Variables.SV;
                    NewVersion = Manifest.latest.snapshot;
                    break;
                case 'b': // Both
                    Update("vanilla");
                    Update("snapshot");
                    return;
                case 'l':
                    currentVersion = Version;
                    NewVersion = currentVersion;
                    break;
            }

            if (currentVersion == NewVersion && File.Exists(LauncherPath))
                return;
            else if (!File.Exists(LauncherPath))
                Console.WriteLine($"The version {NewVersion} has not been downloaded, would you like to download it?");
            else
                Console.WriteLine($"There is a new version available, {NewVersion}, would you like to download it?");
            Console.Write("[Y]/N: ");
            bool getNewest = Funcs.ValidateBool(true);
            if (getNewest)
            {
                if (!File.Exists(LauncherPath))
                {
                    Download();
                }
                else
                {
                    UpdateVersions();
                    Console.ReadLine();
                }
                ChangeVersion(NewVersion);
            }
        }

        /// <summary>
        /// Downloads the latest minecraft_server.*.jar for the appropriate version
        /// </summary>
        private void Download()
        {
            // Gets the first (or empty) version object from the json
            Data.Version newServerJSON = Manifest.versions.DefaultIfEmpty(new Data.Version { url = "" }).FirstOrDefault(i => i.id == NewVersion);

            // If the url exists
            if (newServerJSON != null)
            {
                // The url for the server
                string newServerURL = JsonConvert.DeserializeObject<VersionJSON>(parseJSON(newServerJSON.url)).downloads.server.url;

                // Downloads and saves the file in the correct spot
                GetFile(newServerURL, LauncherPath);

                Console.WriteLine($"Downloaded minecraft_server.{NewVersion}.jar");

                UpdateVersions();
            }
            else
            {
                Console.WriteLine("The specified file could not be found or did not exist.");
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Updates versions
        /// </summary>
        private void UpdateVersions()
        {
            // Even if we are not changing the text files, print output stating that the version was updated

            // A copy of the versions file in dictionary form
            switch (ServerType.ToLower().First<char>()) // Updates the version in the dictionary
            {
                case 'v': // Vanilla
                    Console.WriteLine($"Updated vanilla servers from {Variables.VV} to {NewVersion}");
                    Variables.UpdateVersion(ServerVersionTypes.VV, NewVersion);
                    break;
                case 's': // Snapshot
                    Console.WriteLine($"Updated snapshot servers from {Variables.SV} to {NewVersion}");
                    Variables.UpdateVersion(ServerVersionTypes.SV, NewVersion);
                    break;
            }
        }

        /// <summary>
        /// Parses json into C# objects from a url
        /// </summary>
        /// <param name="url">url with json</param>
        /// <returns></returns>
        private static string parseJSON(string url)
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString(url);
                wc.Dispose();
                return json;
            }
        }

        /// <summary>
        /// Downloads a file to specified path
        /// </summary>
        /// <param name="url">file to get</param>
        /// <param name="path">file to save</param>
        private static void GetFile(string url, string path)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(url, path);
                wc.Dispose();
            }
        }
    }
}
