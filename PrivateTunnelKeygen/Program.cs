using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static PrivateTunnelKeygen.ConsoleHelper;

namespace PrivateTunnelKeygen
{
    static class Program
    {
        private const int UsersCacheSize = 1;
        private const int SignUpAttempts = 6;
        private const int TrafficUpdatePeriod = 15 * 1000;

        static async Task Main(string[] args)
        {
            Credentials user;
            CookieContainer cookies;

            if (!Cache.TryGetCredentials(out user, out cookies, out int usersInCache))
                (user, cookies) = await SignUp();

            FileManager.InjectCredentials(user);
            ProcessManager.RestartClient();

            Task.WaitAll(
                Enumerable.Range(0, UsersCacheSize - usersInCache)
                    .Select(i => SignUp())
                    .ToArray()
            );

            while (!ExitRequested())
            {
                var (left, total) = await PrivateTunnel.GetTraffic(user.Email, cookies);
                WriteLine($"Left: %Green{left}%, Total: %Green{total}%");
                await Task.Delay(TrafficUpdatePeriod);
                
            }

            bool ExitRequested()
            {
                while (Console.KeyAvailable)
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        return true;

                return false;
            }
        }

        static async Task<(Credentials credentials, CookieContainer cookies)> SignUp()
        {
            for (int i = 0; i < SignUpAttempts; i++)
            {
                try
                {
                    var user = await TempMail.CreateRandomUser();
                    var cookies = await PrivateTunnel.SignUp(user);

                    Cache.SaveCredentials(user, cookies);

                    return (user, cookies);
                }
                catch (Exception) { }
            }

            return (null, null);
        }
    }
}