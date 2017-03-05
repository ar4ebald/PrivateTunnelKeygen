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
        private static string GetConfigPath()
        {
            string path;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                path = @"%localappdata%\PrivateTunnel\ptcore.cfg";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                path = @"~/Library/Application Support/PrivateTunnel/ptcore.cfg";
            else
                throw new NotSupportedException("PrivateTunnel is unavailable for linux yet");

            path = Environment.ExpandEnvironmentVariables(path);

            if (File.Exists(path))
                return path;

            throw new FileNotFoundException("Unable to find PrivateTunnel configuration file.");
        }

        public static string GetExecutablePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string[] prefixes = { "%ProgramFiles%", "%ProgramFiles(x86)%" };
                foreach (var prefix in prefixes.Select(Environment.ExpandEnvironmentVariables))
                {
                    string folder = $@"{prefix}\OpenVPN Technologies\PrivateTunnel";
                    if (Directory.Exists(folder))
                    {
                        string path = Directory.GetFiles(folder, "PrivateTunnel*.exe").FirstOrDefault();
                        if (path != null)
                            return path;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "/Applications/PrivateTunnel.app";
            else
                throw new NotSupportedException("PrivateTunnel is unavailable for linux yet");

            throw new FileNotFoundException("Unable to find PrivateTunnel executable file.");
        }

        public static DirectoryInfo GetUsersCacheFolderPath()
        {
            string path;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                path = @"%appdata%\PrivateTunnelKeygen\UsersCache";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                path = @"~/Library/Application Support/PrivateTunnel/UsersCache";
            else
                throw new NotSupportedException("PrivateTunnel is unavailable for linux yet");

            return Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(path));
        }

        public static void InjectCredentials(TempMail.Credentials credentials)
        {
            const string emailKey = "[username]";
            const string passwordKey = "[password]";

            WriteLine("Injecting credentials...");

            string configPath = GetConfigPath();

            List<string> lines = File.ReadAllLines(configPath).ToList();

            int i = 0;
            while (i < lines.Count)
            {
                switch (lines[i])
                {
                    case emailKey:
                    case passwordKey:
                    case "[token]":
                        lines.RemoveRange(i, 2);
                        break;
                    case "[auto-connect]":
                        lines[i + 1] = "status=enable";
                        i += 2;
                        break;
                    case "[vpn-permit]":
                        lines[i + 1] = "true";
                        i += 2;
                        break;
                    default:
                        i++;
                        break;
                }
            }

            lines.InsertRange(0, new[]
            {
                emailKey, credentials.Email,
                passwordKey, credentials.PasswordHash,
            });

            File.WriteAllLines(configPath, lines);
        }
    }
}
