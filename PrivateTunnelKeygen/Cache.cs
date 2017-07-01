using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace PrivateTunnelKeygen
{
    static class Cache
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public static bool TryGetCredentials(out Credentials user, out CookieContainer cookies, out int usersLeft)
        {
            FileInfo[] files = FileManager.UsersCacheDir.GetFiles();

            (user, cookies) = files
                .OrderBy(i => i.CreationTime)
                .Select(ReadCredentials)
                .FirstOrDefault();

            if (user == null)
            {
                usersLeft = 0;
                return false;
            }
            else
            {
                usersLeft = files.Length - 1;
                return true;
            }
        }

        private static (Credentials, CookieContainer) ReadCredentials(FileInfo file)
        {
            Credentials user;
            CookieContainer cookies;

            using (FileStream stream = file.OpenRead())
            {
                user = (Credentials)Formatter.Deserialize(stream);

                cookies = new CookieContainer();
                foreach (var cookie in (Cookie[])Formatter.Deserialize(stream))
                    cookies.Add(cookie);
            }

            file.Delete();

            return (user, cookies);
        }


        public static void SaveCredentials(Credentials user, CookieContainer cookies)
        {
            string path = Path.Combine(FileManager.UsersCacheDir.FullName, Guid.NewGuid().ToString("N"));
            using (FileStream stream = File.OpenWrite(path))
            {
                Formatter.Serialize(stream, user);
                Formatter.Serialize(stream, GetCookies(cookies).ToArray());
            }
        }

        private static IEnumerable<Cookie> GetCookies(this CookieContainer cookieContainer)
        {
            var domainTable = Util.GetFieldValue<dynamic>(cookieContainer, "domainTable");
            foreach (var entry in domainTable)
            {
                string key = Util.GetPropertyValue<string>(entry, "Key");
                var value = Util.GetPropertyValue<dynamic>(entry, "Value");

                var internalList = Util.GetFieldValue<SortedList<string, CookieCollection>>(value, "list");
                foreach (var li in internalList)
                {
                    foreach (Cookie cookie in li.Value)
                    {
                        yield return cookie;
                    }
                }
            }
        }
    }
}
