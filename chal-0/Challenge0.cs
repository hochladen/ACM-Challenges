using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Challenge0
{
    class Challenge0
    {
        static HttpClient Client = new HttpClient();
        static string Uri = "https://us-central1-acm-core.cloudfunctions.net/challenge/tags/linux";
        static string Token = "";

        static Task<HttpResponseMessage> SendPost()
        {
            string Content = "{\"name\": \"linux\", \"contents\": \"I'd just like to interject for a moment. What you're referring to as Linux is in fact, GNU/Linux, or as I've recently taken to calling it, GNU plus Linux...\"}";
            StringContent StringContent = new StringContent(Content, System.Text.Encoding.UTF8, "application/json");
            return Client.PostAsync(Uri, StringContent);
        }

        static Task<HttpResponseMessage> SendGet()
        {
            return Client.GetAsync(Uri);
        }

        static Task<HttpResponseMessage> SendPatch()
        {
            string Content = "{\"contents\": \"Something else\"}";
            StringContent StringContent = new StringContent(Content, System.Text.Encoding.UTF8, "application/json");
            return Client.PatchAsync(Uri, StringContent);
        }

        static Task<HttpResponseMessage> SendDelete()
        {
            return Client.DeleteAsync(Uri);
        }

        delegate Task<HttpResponseMessage> SendDelegate();

        static Dictionary<string, SendDelegate> RequestDictionary = new Dictionary<string, SendDelegate>() 
        {
            { "POST", SendPost },
            { "GET", SendGet },
            { "PATCH", SendPatch },
            { "DELETE", SendDelete }
        };

        static void Main(string[] args)
        {
            foreach (var Pair in RequestDictionary)
            {
                Console.WriteLine($"Sending {Pair.Key} Request\n");
                var SendTask = Pair.Value();
                string RequestType = Pair.Key;
                SendTask.Wait();
                if (RequestType != "DELETE")
                {
                    var ReadTask = SendTask.Result.Content.ReadAsStringAsync();
                    ReadTask.Wait();
                    JObject JSONObject;
                    try
                    {
                        JSONObject = JObject.Parse(ReadTask.Result);
                    }
                    catch (Newtonsoft.Json.JsonReaderException jre)
                    {
                        Console.Error.WriteLine($"Error parsing JSON from string:\n${jre.Message}\n");
                        continue;
                    }
                    if (Token.Length == 0 && JSONObject.ContainsKey("token"))
                    {
                        Token = JSONObject.Value<string>("token");
                        Uri += $"/{Token}";
                    }
                    Console.WriteLine($"{RequestType} Response:\n{ReadTask.Result}\n");
                } else
                {
                    Console.WriteLine($"{RequestType} Response:\n{SendTask.Result.StatusCode}\n");
                }
            }
            Console.WriteLine("\nDone!\n\nPress any key to exit");
            Console.ReadKey();
        }
    }
}