using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WP_User_Enumeration
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Enumerator();
        }

        private static void Enumerator()
        {
            while (true)
            {
                Console.Clear();

                Uri url;
                string urlString;
                var resultList = new List<string>();

                Console.WriteLine("\n|==========| Wordpress Username Enumerator |==========|\n");

                Console.Write("Target URL: ");
                urlString = Console.ReadLine();

                if (urlString[..4] != "http") urlString = "http://" + urlString;
                url = new Uri(urlString);

                Console.WriteLine("\n[+] Enumerating all users...");

                try
                {
                    using var client = new WebClient();
                    var response = client.DownloadString(new Uri(url, @"/wp-json/wp/v2/users/"));
                    var jobject = (JArray) JsonConvert.DeserializeObject(response);

                    if (jobject != null && jobject.Count > 0)
                        resultList.AddRange(jobject.Select(token => token["slug"].Value<string>()));
                }

                catch (Exception e)
                {
                    Console.WriteLine($"[!] ERROR: {e.Message}");
                    Console.WriteLine("This website might not be vulnerable to this scan! Click any key...");
                    Console.ReadKey();
                }

                if (resultList.Count > 0) File.WriteAllLines(url.DnsSafeHost, resultList);

                Console.WriteLine($"\r\n[!] Founded usernames: {string.Join(',', resultList)}");
                Console.WriteLine($"[!] Total {resultList.Count} usernames saved to file. Click any key...");
                Console.ReadKey();
            }
        }
    }
}