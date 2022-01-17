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
            Enumerator(args);
        }

        private static void Enumerator(IReadOnlyList<string> args)
        {
            while (true)
            {
                Console.Clear();

                string urlString;
                var resultList = new List<string>();

                Console.WriteLine("\n|==========| Wordpress Username Enumerator |==========|\n");

                if (args == null || args.Count == 0)
                {
                    Console.Write("Target URL: ");
                    urlString = Console.ReadLine();
                }
                else
                    urlString = args[0];


                if (string.IsNullOrEmpty(urlString)) return;

                if (urlString[..4] != "http") urlString = "http://" + urlString;
                var url = new Uri(urlString);

                Console.WriteLine("\n[+] Enumerating all users...");

                try
                {
                    using var client = new WebClient();
                    var response = client.DownloadString(new Uri(url, @"/wp-json/wp/v2/users/"));
                    var jobject = (JArray) JsonConvert.DeserializeObject(response);

                    if (jobject is {Count: > 0})
                        resultList.AddRange(jobject.Select(token => token["slug"].Value<string>()));
                }

                catch (Exception e)
                {
                    Console.WriteLine($"[!] ERROR: {e.Message}");
                    Console.WriteLine( "[!] This website might not be vulnerable to this scan! Click any key...");
                    Console.ReadKey();
                }

                if (resultList.Count > 0) File.WriteAllLines(url.DnsSafeHost+".txt", resultList);

                Console.WriteLine($"\r\n[!] Logins found: {string.Join(',', resultList)}");
                Console.WriteLine($"[!] Total {resultList.Count} logins saved to file. Click any key...");
                Console.ReadKey();
                args = null;
            }
        }
    }
}