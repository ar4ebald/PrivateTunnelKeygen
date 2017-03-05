using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using static PrivateTunnelKeygen.ConsoleHelper;

namespace PrivateTunnelKeygen
{
    static class Api
    {
        public static async Task<CookieContainer> SignUp(TempMail.Credentials user, CancellationToken token)
        {
            var timer = Stopwatch.StartNew();

            string encodedEmail = WebUtility.UrlEncode(user.Email);
            string encodedPassword = WebUtility.UrlEncode(user.Password.ToBase64());

            string request =
                $"https://www.privatetunnel.com/jsonapi25/ptuserAPI.php" +
                $"?email={encodedEmail}" +
                $"&email2={encodedEmail}" +
                $"&request=register&tos=Yes" +
                $"&epassword={encodedPassword}" +
                $"&callback=angular.callbacks._0";

            var cookies = new CookieContainer();

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookies
            }))
            {
                WriteLine($"{user.Email}: Registering the new account...");
                await client.GetAsync(request, token);
                token.ThrowIfCancellationRequested();

                WriteLine($"{user.Email}: Awaiting for the confirmation link...");
                string[] mails;
                while ((mails = await TempMail.GetMail(user.Email, token)) == null)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(1000, token);
                }

                WriteLine($"{user.Email}: Confirming the email...");
                string confirmUrl = Regex.Match(mails[0], @"(?<=a\s+href\s*=\s*"").*?(?="")").Value;
                await client.GetAsync(confirmUrl, token);
                token.ThrowIfCancellationRequested();
            }

            Write($"{user.Email}: User registered in ");
            Write($"{timer.Elapsed.TotalSeconds:F2}", ConsoleColor.Green);
            WriteLine(" seconds!");

            return cookies;
        }

        public static void GetTraffic(string email, CookieContainer cookies, out string left, out string total)
        {
            string request =
                $"https://www.privatetunnel.com/jsonapi25/ptuserAPI.php?request=currentdata&callback=hacked" +
                $"&username={WebUtility.UrlEncode(email)}";

            string response;
            using (var client = new HttpClient(new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookies
            }))
                response = client.GetStringAsync(request).Result;

            const string responseStart = "hacked([";
            const string responseEnd = "])";

            response =
                response.Substring(
                    responseStart.Length,
                    response.Length - (responseStart.Length + responseEnd.Length));

            var json = JObject.Parse(response)["userInit"];

            left = json.Value<string>("credit_curr");
            total = json.Value<string>("credit_total");
        }
    }
}
