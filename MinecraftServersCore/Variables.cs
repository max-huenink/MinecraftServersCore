using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;

namespace MinecraftServersCore
{
    enum ServerVersionTypes { VV, SV }
    class Variables
    {
        /// <summary>
        /// The location of the executable
        /// </summary>
        public static string sFiles { get; protected set; }
        /// <summary>
        /// Version Files Dir (inside server files)
        /// </summary>
        public static string vFiles { get; protected set; }
        /// <summary>
        /// Vanilla servers directory
        /// </summary>
        public static string dirsVANILLA { get; protected set; }

        /// <summary>
        /// Vanilla version
        /// </summary>
        public static string VV { get; protected set; }
        /// <summary>
        /// Snapshot version
        /// </summary>
        public static string SV { get; protected set; }

        /// <summary>
        /// A Dictionary<string,string> containing the version for all server types
        /// </summary>
        //public static Dictionary<string, string> versions { get; protected set; }
        public static XmlDocument versions { get; protected set; }
        //private static XmlDocument versionsDoc;

        /// <summary>
        /// The path to the versions.txt file
        /// </summary>
        public static string versionsPath { get; protected set; }

        /// <summary>
        /// Sets various variables for use throughout the program
        /// </summary>
        static Variables()
        {
            // The location of the executable i.e. C:\Servers\Files
            string files_origin = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // The parent directory of the executable directory i.e. C:\Servers
            string servers_origin = Path.GetDirectoryName(files_origin);

            sFiles = files_origin;
            vFiles = Path.GetDirectoryName(Path.Combine(sFiles, "versions/"));
            dirsVANILLA = Path.GetDirectoryName(Path.Combine(servers_origin, "minecraftServers/"));

            // Ensures all directories exist on each run
            foreach (string dir in new string[] { vFiles, dirsVANILLA, })
            {
                Directory.CreateDirectory(dir);
            }

            // Each server type's version
            versionsPath = Path.Combine(vFiles, "versions.xml");
            if (File.Exists(versionsPath))
            {
                LoadVariables();
            }
            else
            {
                versions = new XmlDocument();
                versions.LoadXml("<?xml version='1.0' encoding='utf-8'?><versions></versions>");
                versions.Save(versionsPath);
            }
        }

        /// <summary>
        /// Updates the server version variables obtained through `versions.txt`
        /// </summary>
        public static void LoadVariables()
        {
            versions = new XmlDocument();
            versions.Load(versionsPath);

            // Vanilla
            VV = GetVersionNodeValue(ServerVersionTypes.VV);

            //Snapshot
            SV = GetVersionNodeValue(ServerVersionTypes.SV);
        }

        /// <summary>
        /// Gets the version of the server type
        /// </summary>
        /// <param name="type">The type of server (VV, SV)</param>
        /// <returns>A string representing the version</returns>
        public static string GetVersionNodeValue(ServerVersionTypes type) => GetVersionNode(type).InnerText;

        /// <summary>
        /// Gets the version xml node for the server type
        /// </summary>
        /// <param name="type">The type of server (VV, SV)</param>
        /// <returns>A version xml node for the server type</returns>
        private static XmlNode GetVersionNode(ServerVersionTypes type)
        {
            XmlNode ver = versions.SelectSingleNode($"versions/version[@name='{type}']");
            if (ver == null)
            {
                ver = versions.CreateNode(XmlNodeType.Element, "version", "");
                XmlAttribute attribute = versions.CreateAttribute("name");
                attribute.Value = type.ToString();
                ver.Attributes.Append(attribute);
                versions.SelectSingleNode("/versions").AppendChild(ver);
            }
            return ver;
        }

        /// <summary>
        /// Updates the server version in the xml file
        /// </summary>
        /// <param name="type">The type of server (VV, SV)</param>
        /// <param name="version">The new server version</param>
        public static void UpdateVersion(ServerVersionTypes type, string version)
        {
            XmlNode node = GetVersionNode(type);
            node.InnerText = version;
            versions.Save(versionsPath);
        }
    }
}
