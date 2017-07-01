using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using static PrivateTunnelKeygen.ConsoleHelper;

namespace PrivateTunnelKeygen
{
    static class PrivateTunnel
    {
        public static async Task<CookieContainer> SignUp(Credentials user)
        {
            var timer = Stopwatch.StartNew();

            string encodedEmail = WebUtility.UrlEncode(user.Email);

            string request =
                $"https://www.privatetunnel.com/jsonapi25/ptuserAPI.php" +
                $"?email={encodedEmail}&email2={encodedEmail}" +
                $"&request=register&tos=Yes" +
                $"&epassword={WebUtility.UrlEncode(user.Password.ToBase64())}" +
                $"&callback=angular.callbacks._0";

            var cookies = new CookieContainer();

            using (var client = new HttpClient(new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookies
            }))
            {
                WriteLine($"{user.Email}: Registering new account...");
                await client.GetAsync(request);

                WriteLine($"{user.Email}: Awaiting for the confirmation link...");
                string[] mails;
                while ((mails = await TempMail.GetMail(user.Email)) == null)
                    await Task.Delay(1000);

                WriteLine($"{user.Email}: Confirming the email...");
                string confirmUrl = Regex.Match(mails[0], @"(?<=a\s+href\s*=\s*"").*?(?="")").Value;
                await client.GetAsync(confirmUrl);
            }

            WriteLine($"{user.Email}: User registered in %Green{timer.Elapsed.TotalSeconds:F2}% seconds!");

            return cookies;
        }

        public static async Task<(string left, string total)> GetTraffic(string email, CookieContainer cookies)
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
                response = await client.GetStringAsync(request);

            const string responseStart = "hacked([";
            const string responseEnd = "])";

            response =
                response.Substring(
                    responseStart.Length,
                    response.Length - (responseStart.Length + responseEnd.Length));

            var json = JObject.Parse(response)["userInit"];

            return (json.Value<string>("credit_curr"), json.Value<string>("credit_total"));
        }
    }
}
