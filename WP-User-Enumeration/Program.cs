using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
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
            Enumerator();   
        }

        static void Enumerator()
        {
            while (true)
            {
                Console.Clear();

                Uri url;
                string path, urlString;
                int maxFails, failsCounter = 0, idCounter = 1;
                var resultList = new List<string>();

                Console.WriteLine("\n|==========| Wordpress Username Enumerator |==========|\n");

                Console.Write("Target URL: ");
                urlString = Console.ReadLine();

                if (urlString[..4] != "http") urlString = "http://" + urlString;

                url = new Uri(urlString);

                using (var checkVulnerability = new WebClient())
                {
                    try
                    {
                        checkVulnerability.DownloadString(new Uri(url, "/wp-json/wp/v2/users/"));
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                        {
                            var resp = (HttpWebResponse) ex.Response;

                            if (resp.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden) Console.WriteLine($"[!] Website \"{url.AbsoluteUri}\" is not vulnerable to this scan!");
                        }

                        else
                            Console.WriteLine($"[!] Error: {ex.Message}");

                        Console.ReadKey();
                        Enumerator();
                    }
                }

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
                            var jobject = (JObject) JsonConvert.DeserializeObject(response);
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

                if (resultList.Count > 0) File.WriteAllLines(path, resultList);

                Console.WriteLine($"\r\n[!] Complete! Found {resultList.Count} usernames");
                Console.ReadKey();
            }
        }
    }
}
