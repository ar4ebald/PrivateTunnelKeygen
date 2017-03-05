using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PrivateTunnelKeygen
{
    class Program
    {
        private const int UsersCacheSize = 5;

        static void Main(string[] args)
        {
            Run().Wait();
        }

        static async Task<TempMail.Credentials> SignUp()
        {
            var source = new CancellationTokenSource();
            var user = await TempMail.CreateRandomUser(source.Token);
            var cookies = await Api.SignUp(user, source.Token);
            return user;
        }

        static async Task Run()
        {
            TempMail.Credentials user;

            var cacheFolder = FileManager.GetUsersCacheFolderPath();
            var cachedUsers = cacheFolder.GetFiles().ToList();

            if (cachedUsers.Count == 0)
                user = await SignUp();
            else
            {
                var oldestFile = cachedUsers.OrderBy(i => i.CreationTime).First();

                user = JToken.Parse(File.ReadAllText(oldestFile.FullName)).ToObject<TempMail.Credentials>();

                cachedUsers.Remove(oldestFile);
                oldestFile.Delete();
            }

            FileManager.InjectCredentials(user);
            ProcessManager.RestartClient();

            var workAmmount = UsersCacheSize - cachedUsers.Count;

            Parallel.For(0, workAmmount, i =>
            {
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        var newUser = SignUp().Result;
                        var path = Path.Combine(cacheFolder.FullName, Guid.NewGuid().ToString("N"));
                        File.WriteAllText(path, JToken.FromObject(newUser).ToString());

                        lock (cachedUsers)
                            cachedUsers.Add(new FileInfo(path));

                        ConsoleHelper.WriteLine($"Cached {cachedUsers.Count} users out of {UsersCacheSize}");
                        return;
                    }
                    catch (HttpRequestException) { }
                }
            });
        }
    }
}