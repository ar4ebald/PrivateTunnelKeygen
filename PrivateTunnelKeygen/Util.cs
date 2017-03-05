using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PrivateTunnelKeygen
{
    static class Util
    {
        private static readonly Random Random = new Random();

        public static string ToBase64(this string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public static T RandomItem<T>(this T[] array)
        {
            return array[Random.Next(array.Length)];
        }

        public static async Task<JToken> GetJsonAsync(this HttpClient client, string url, CancellationToken token)
        {
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseContentRead, token);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JToken.Parse(content);
        }
    }
}
