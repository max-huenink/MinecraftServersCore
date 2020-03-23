using System;
using System.Diagnostics;
using System.Linq;

namespace MinecraftServersCore
{
    class Program
    {
        public static void Main(string[] args)
        {
            string action = Assign(args, 0); // What the action is launch/create/update
            string arg1 = Assign(args, 1); // serverName or serverType
            string arg2 = Assign(args, 2); // startType or newVersion
            string version = Assign(args, 3); // The server version (applicable if launchType==legacy)

            if (action == "") // Gets program action
            {
                Console.WriteLine("Would you like to launch, create, or update?");
                Console.Write("Action [launch]: ");
                action = Funcs.NullEmpty("launch");
            }

            // Both serverName and serverType are arg1, context depending
            string serverName = arg1, serverType = serverName;
            // serverName: The name of the server
            // serverType: The type of server (vanilla/snapshot/both)

            // Both launchType and newVersion are arg2, context depending
            string launchFile = arg2, newVersion = launchFile;
            // launchFile: The launch file the server should use (backup/restore/legacy/default)
            // newVersion: The new version to update to

            // Create the server object
            Server server = new Server(serverName, launchFile, version);
            switch (action.ToLower().First<char>())
            {
                case 'l': // Launch server
                    server.Launch();
                    break;
                case 'c': // Create server, then launch server
                    server.Create();
                    goto case 'l';
                case 'u': // Update specified version
                    server.Update(serverType, newVersion);
                    break;
            }
        }

        /// <summary>
        /// Get the nth arg, "" if past out of bounds
        /// </summary>
        /// <param name="args">The args array</param>
        /// <param name="indice">The indice for the array</param>
        /// <returns>string</returns>
        private static string Assign(string[] args, int indice)
        {
            int len = args.Length;
            int expected_length = indice + 1;

            if (expected_length <= len)
            {
                return args[indice];
            }
            else
            {
                return "";
            }
        }
    }
}
