using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MinecraftServersCore
{
    public partial class Server
    {
        /// <summary>
        /// The name of the Server
        /// </summary>
        public string ServerName { get; protected set; }

        /// <summary>
        /// The file the server launches from backup/restore/legacy/default
        /// </summary>
        public string LaunchType { get; protected set; }

        /// <summary>
        /// The Server's version
        /// </summary>
        public string Version { get; protected set; }

        /// <summary>
        /// The classification of the server, vanilla, snapshot, legacy
        /// </summary>
        public string ServerType { get; protected set; }

        /// <summary>
        /// The directory for the Server
        /// </summary>
        public string ServerDir => Path.Combine(ParentDir, ServerName);

        /// <summary>
        /// A directory containing servers
        /// </summary>
        public readonly string ParentDir;

        /// <summary>
        /// The location of the java executable for the server
        /// </summary>
        public string LaunchJar => Path.Combine(Variables.vFiles, $"minecraft_server.{Version}.jar");

        /// <summary>
        /// The server
        /// </summary>
        /// <param name="serverName">The name of the server</param>
        /// <param name="launchType">The launch type of the server (backup/restore/legacy/default)</param>
        /// <param name="version">The version of the server (used if legacy is specified)</param>
        public Server(string serverName, string launchType, string version)
        {
            ServerName = serverName;
            LaunchType = launchType;
            Version = version;
            ParentDir = Variables.dirsVANILLA;
        }

        /// <summary>
        /// Launches the server
        /// </summary>
        public void Launch()
        {
            // Gets the name of the Server
            while (ServerName == "")
            {
                Console.Clear();
                Console.WriteLine("----------");
                IEnumerable<string> folders = Directory.GetDirectories(ParentDir).Select(i => Path.GetFileName(i));
                Funcs.PrettyPrint(folders);
                Console.WriteLine("\nWhich server would you like to start?");
                Console.Write("Server Name: ");
                ServerName = Console.ReadLine();
                if (string.IsNullOrEmpty(ServerName))
                {
                    Console.WriteLine("You must specify a name.");
                    Console.ReadLine();
                }
            }

            // If the directory exists, launch
            if (Directory.Exists(ServerDir))
            {
                // Sets current directory to that of the selected server
                Directory.SetCurrentDirectory(ServerDir);

                // Asks for the StartType
                if (LaunchType == "")
                {
                    Console.Clear();
                    Console.WriteLine("----------");
                    string[] allFiles = Directory.GetFiles(ServerDir);
                    // Prints all files with .type extension in the server's directory (backup, restore, default, legacy)
                    List<string> files = new List<string>();
                    foreach (string file in allFiles)
                    {
                        if (Path.GetExtension(file) == ".type")
                        {
                            files.Add(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                    files.Add("legacy (for specific version)");
                    Funcs.PrettyPrint(files);
                    Console.WriteLine("Which start type would you like to use?");
                    Console.Write("Start Type [default]: ");
                    LaunchType = Funcs.NullEmpty("default");

                    if (LaunchType.ToLower().First<char>() != 'l') // If the launch file isn't legacy
                    {
                        IEnumerable<string> fileLines = new string[] { "vanilla" };
                        if (File.Exists(Path.Combine(ServerDir, "default.type")))
                        {
                            fileLines = File.ReadLines(Path.Combine(ServerDir, "default.type"));
                        }
                        switch (fileLines.DefaultIfEmpty("vanilla").FirstOrDefault().ToLower().First<char>())
                        {
                            case 'v': // vanilla
                                ServerType = "vanilla";
                                Version = Variables.VV;
                                break;
                            case 's': // snapshot
                                ServerType = "snapshot";
                                Version = Variables.SV;
                                break;
                            case 'c': // custom
                                ServerType = "legacy"; // Version will be set before starting server
                                break;
                            default: // custom (predefined): i.e. 1.8.9
                                ServerType = "legacy";
                                Version = fileLines.First();
                                break;
                        }
                    }
                }
            }
            // If the server doesn't exist, ask if we should create it
            else
            {
                Console.WriteLine($"The server {ServerName}, does not exist. Would you like to create it?");
                Console.Write("Create? Y/[N]: ");
                bool input = Funcs.ValidateBool(false);
                // Create the server, or exit program
                if (input)
                {
                    Create();
                }
                else
                {
                    return;
                }
            }
            // Start the server
            Start();
        }

        /// <summary>
        /// Creates the server
        /// </summary>
        public void Create()
        {
            LaunchType = "default";

            // Set ServerName (exits if name is blank)
            while (ServerName == "")
            {
                Console.WriteLine("What is the server name?");
                Console.Write("Name: ");
                ServerName = Console.ReadLine();
                if (string.IsNullOrEmpty(ServerName))
                {
                    Console.WriteLine("You must specify a name.");
                    Console.ReadLine();
                    continue;
                }
                if (Directory.Exists(ServerDir))
                {
                    Console.WriteLine("That server already exists, enter a new server name.");
                    ServerName = "";
                }
            }

            // Create and go to directory
            Directory.CreateDirectory(ServerDir);
            Directory.SetCurrentDirectory(ServerDir);

            // Get text to put in `default.type`
            Console.Clear();
            Console.WriteLine("----------");
            Console.WriteLine("Launch types: ");
            Funcs.PrettyPrint(new string[] { "vanilla", "snapshot", "custom (1.8.9, 1.13.2, etc.)" });
            Console.WriteLine("Launch type [vanilla]: ");
            string launchType = Funcs.NullEmpty("vanilla");

            // Make .type files
            Write("backup.type", "");
            Write("restore.type", "");
            Write("default.type", launchType); // vanilla, snapshot, custom {1.8.9,1.13.1,etc.}

            // The eula has to be accepted before the server can run, automatically accept this
            Write("eula.txt", "eula=true");

            // Op and Whitelist self
            string myUID = "1b7ffd7a-6c9a-463e-8910-60c7a531b2a4";
            string myName = "maxh76";
            Write("ops.json", $@"[{{ ""uuid"":""{myUID}"", ""name"":""{myName}"", ""level"":4, ""bypassesplayerlimit"":true }}]");
            Write("whitelist.json", $@"[{{ ""uuid"":""{myUID}"", ""name"":""{myName}"" }}]");
            /*
             * These files are created when the server runs for the first time.
             * Since they are empty we can let the server handle that
            Write("banned-ips.json", false, $"[]");
            Write("banned-players.json", false, $"[]");
            Write("usercache.json", false, $"[]");
            */

            // Copy the server icon to the directory (if it exists)
            string iconName = "server-icon.png";
            string iconPath = Path.Combine(Variables.sFiles, iconName);
            if (File.Exists(iconPath))
            {
                File.Copy(iconPath, Path.Combine(ServerDir, iconName), true);
            }
            else
            {
                Console.WriteLine(@"No server icon (""server-icon.png"") exists");
                Console.WriteLine(@"Place an image called server-icon.png in the folder where this executable is stored.");
            }

            // Create/change server.properties and set motd
            Console.WriteLine("Would you like to make settings changes?");
            Console.Write("Changes Y/[N]: ");
            bool changes = Funcs.ValidateBool(false);
            ServerProperties(changes);
        }

        /// <summary>
        /// Ensures the server is running the specified version
        /// </summary>
        /// <param name="version">The Minecraft version to use when launching</param>
        public void ChangeVersion(string version)
        {
            Version = version;
        }

        /// <summary>
        /// Writes text to a file in the server directory
        /// If the file does not exist, a new file is created.
        /// </summary>
        /// <param name="rawPath">The name of the file to write to (serverDir is prepended in this method)</param>
        /// <param name="textToWrite">The string to write to the file</param>
        private void Write(string rawPath, string text) => Funcs.Write(Path.Combine(ServerDir, rawPath), false, text);

        /// <summary>
        /// Creates the server.properties file
        /// </summary>
        /// <param name="changes">True to make changes, False to use defaults</param>
        private void ServerProperties(bool changes)
        {
            int gamemode = 0;
            int difficulty = 1;
            bool hardcore = false;
            bool whitelist = false;
            string motd = GetMotd("Max\'s Minecraft Server");

            if (changes)
            {
                Console.WriteLine("Gamemodes\n0: Survival, 1: Creative, 2: Adventure, 3: Spectator");
                Console.Write("Gamemode (0-3) [0]: ");
                gamemode = Funcs.GetInt(0, 3, 0);

                Console.WriteLine("Difficulties\n0: Peaceful, 1:Easy, 2: Normal, 3: Hard");
                Console.Write("Difficulty (0-3) [1]: ");
                difficulty = Funcs.GetInt(0, 3, 1);

                Console.Write("Hardcore true/[false]: ");
                hardcore = Funcs.ValidateBool(false);

                Console.Write("Whitelist true/[false]: ");
                whitelist = Funcs.ValidateBool(false);
            }

            string properties = "spawn-protection=0" +
            "max-tick-time=60000\n" +
            "generator-settings=\n" +
            "allow-nether=true\n" +
            "force-gamemode=false\n" +
            "enforce-whitelist=false\n" +
            $"gamemode={gamemode}\n" +
            "broadcast-console-to-ops=true\n" +
            "enable-query=true\n" +
            "player-idle-timeout=5\n" +
            $"difficulty={difficulty}\n" +
            "spawn-monsters=true\n" +
            "op-permission-level=3\n" +
            "pvp=true\n" +
            "snooper-enabled=true\n" +
            "level-type=DEFAULT\n" +
            $"hardcore={hardcore}\n" +
            "enable-command-block=true\n" +
            "max-players=20\n" +
            "network-compression-threshold=256\n" +
            "resource-pack-sha1=\n" +
            "max-world-size=29999984\n" +
            "server-port=25565\n" +
            "server-ip=\n" +
            "spawn-npcs=true\n" +
            "allow-flight=false\n" +
            $"level-name={ServerName}\n" +
            "view-distance=10\n" +
            "resource-pack=\n" +
            "spawn-animals=true\n" +
            $"white-list={whitelist}\n" +
            "generate-structures=true\n" +
            "online-mode=true\n" +
            "max-build-height=256\n" +
            "level-seed=\n" +
            "prevent-proxy-connections=false\n" +
            $"motd={@motd}\n" +
            "enable-rcon=false";

            Write("server.properties", properties);
        }

        /// <summary>
        /// Returns the motd for server.properties
        /// </summary>
        /// <param name="def">The default motd</param>
        /// <returns></returns>
        private string GetMotd(string def)
        {
            Console.WriteLine("Message of the day:\n\\u00A7");
            Console.WriteLine("Black: 0 | Dark Blue: 1 | Dark Green: 2 | Dark Aqua: 3 | Dark Red: 4\n" +
                                "Dark Purple: 5 | Gold: 6 | Gray: 7 | Dark Gray: 8 | Blue: 9 | Green: a\n" +
                                "Aqua: b | Red: c | Light Purple: d | Yellow: e | White: f\n" +
                                "Obfuscated: k | Bold: l | Strikethrough: s | Underline: n | Italic: o | Reset: r");
            return Funcs.NullEmpty(def);
        }
    }
}
