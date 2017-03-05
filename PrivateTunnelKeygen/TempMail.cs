using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static PrivateTunnelKeygen.ConsoleHelper;

namespace PrivateTunnelKeygen
{
    static class TempMail
    {
        private const string ApiUriRoot = "http://api.temp-mail.org/request";

        private const int EmailNameLength = 8;
        private static readonly char[] EmailCharacters = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        private const string Password = "1234";
        private const string PasswordHash = @"Oz+yVPL1qUpG8BFAgrOyGOvsKr15UuBTbjwSl4dt3dy235dc40f50jwTYYGnrzivEtIisEwYBhqB8rDERRzU7fj+OEoD5ZbJ1AeVCAcOQ6Cw07+GIaQmkUAmQCLR6p1wcA0QgSNxhjTtoHtItvRsPWpk9g/iIvYC7CwEJ03aM/g=";

        private static readonly MD5 HashingAlgorithm = MD5.Create();


        private static string CalculateMD5Hash(string input)
        {
            byte[] hash = HashingAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            char[] str = new char[hash.Length * 2];
            for (int i = 0; i < hash.Length; i++)
            {
                char[] hex = hash[i].ToString("x2").ToCharArray();
                Array.Copy(hex, 0, str, i * 2, length: 2);
            }
            return new string(str);
        }

        private static async Task<string[]> GetDomains(CancellationToken token)
        {
            using (var client = new HttpClient())
                return (await client.GetJsonAsync(ApiUriRoot + "/domains/format/json", token))
                    .Select(i => i.Value<string>())
                    .ToArray();
        }

        public static async Task<Credentials> CreateRandomUser(CancellationToken token)
        {
            char[] name = new char[EmailNameLength];
            for (int i = 0; i < EmailNameLength; i++)
                name[i] = EmailCharacters.RandomItem();

            string[] domains = await GetDomains(token);
            string email = new string(name) + domains.RandomItem();

            Write("Email: ");
            WriteLine(email, ConsoleColor.Green);
            Write("Password: ");
            WriteLine(Password, ConsoleColor.Green);

            return new Credentials(email, Password, PasswordHash);
        }

        public static async Task<string[]> GetMail(string email, CancellationToken token)
        {
            string emailHash = CalculateMD5Hash(email);
            string requestUrl = $"{ApiUriRoot}/mail/id/{emailHash}/format/json/";

            try
            {
                using (var client = new HttpClient())
                {
                    var mail = await client.GetJsonAsync(requestUrl, token);
                    return mail.Select(i => i.Value<string>("mail_text")).ToArray();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class Credentials
        {
            public Credentials(string email, string password, string passwordHash)
            {
                Email = email;
                Password = password;
                PasswordHash = passwordHash;
            }

            public string Email { get; }
            public string Password { get; }
            public string PasswordHash { get; }
        }
    }
}
