using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq;

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

        public static async Task<JToken> GetJsonAsync(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JToken.Parse(content);
        }

        public static T GetFieldValue<T>(object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo fi = instance.GetType().GetFields(bindFlags).First(i => i.Name.EndsWith(fieldName));
            return (T)fi.GetValue(instance);
        }

        public static T GetPropertyValue<T>(object instance, string propertyName)
        {
            var pi = instance.GetType().GetProperties().First(i => i.Name.EndsWith(propertyName));
            return (T)pi.GetValue(instance, null);
        }
    }
}
