using System;
using System.Collections.Generic;
using System.Text;

namespace MinecraftServersCore.Data
{
    public class Latest
    {
        public string release { get; set; }
        public string snapshot { get; set; }
    }

    public class Version
    {
        public string id { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public DateTime time { get; set; }
        public DateTime releaseTime { get; set; }
    }

    public class VersionManifest
    {
        public Latest latest { get; set; }
        public List<Version> versions { get; set; }
    }

    public class Client
    {
        public string sha1 { get; set; }
        public int size { get; set; }
        public string url { get; set; }
    }

    public class Server
    {
        public string sha1 { get; set; }
        public int size { get; set; }
        public string url { get; set; }
    }

    public class Downloads
    {
        public Client client { get; set; }
        public Server server { get; set; }
    }

    public class VersionJSON
    {
        public string assets { get; set; }
        public Downloads downloads { get; set; }
        public string id { get; set; }
        public string mainClass { get; set; }
        public int minimumLauncherVersion { get; set; }
        public DateTime releaseTime { get; set; }
        public DateTime time { get; set; }
        public string type { get; set; }
    }
}
