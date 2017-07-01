using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using static PrivateTunnelKeygen.ConsoleHelper;

namespace PrivateTunnelKeygen
{
    static class FileManager
    {
        private static string ConfigurationFilePath
        {
            get
            {
                string path;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    path = @"%localappdata%\PrivateTunnel\ptcore.cfg";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    path = @"%HOME%/Library/Application Support/PrivateTunnel/ptcore.cfg";
                else
                    throw new NotSupportedException("PrivateTunnel is unavailable for linux yet");

                path = Environment.ExpandEnvironmentVariables(path);

                if (File.Exists(path))
                    return path;

                throw new FileNotFoundException("Unable to find PrivateTunnel configuration file.");
            }
        }

        public static string ExecutablePath
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string[] roots = {"%ProgramFiles%", "%ProgramFiles(x86)%"};
                    foreach (var root in roots.Select(Environment.ExpandEnvironmentVariables))
                    {
                        string installDir = $@"{root}\OpenVPN Technologies\PrivateTunnel";
                        if (Directory.Exists(installDir))
                        {
                            string peFile = Directory.GetFiles(installDir, "PrivateTunnel*.exe").FirstOrDefault();
                            if (peFile != null)
                                return peFile;
                        }
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "/Applications/PrivateTunnel.app";
                else
                    throw new NotSupportedException("PrivateTunnel is unavailable for linux yet");

                throw new FileNotFoundException("Unable to find PrivateTunnel executable file.");
            }
        }

        public static DirectoryInfo UsersCacheDir
        {
            get
            {
                string path;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    path = @"%appdata%\PrivateTunnelKeygen\UsersCache";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    path = @"%HOME%/Library/Application Support/PrivateTunnel/UsersCache";
                else
                    throw new NotSupportedException("PrivateTunnel is unavailable for linux yet");

                return Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(path));
            }
        }

        public static void InjectCredentials(Credentials credentials)
        {
            WriteLine("Injecting credentials...");

            List<string> lines = File.ReadAllLines(ConfigurationFilePath).ToList();

            var replaces = new Dictionary<string, string>()
            {
                ["[username]"] = credentials.Email,
                ["[password]"] = credentials.PasswordHash,
                ["[auto-connect]"] = "status=enable",
                ["[vpn-permit]"] = "true"
            };

            for (int i = 0; i < lines.Count; )
            {
                if (lines[i] == "[token]")
                {
                    lines.RemoveRange(i, 2);
                }
                else if (replaces.TryGetValue(lines[i], out string replace))
                {
                    lines[i + 1] = replace;
                    i += 2;
                }
                else
                {
                    i += 1;
                }
            }

            File.WriteAllLines(ConfigurationFilePath, lines);
        }
    }
}
