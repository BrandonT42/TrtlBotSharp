using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace TrtlBotSharp
{
    class Request
    {
        // Sends a JSON POST request to a host wallet
        public static JObject RPC(string Host, int Port, string Method, JObject Params = null, string Password = null)
        {
            JObject Result = new JObject();
            try
            {
                // Create a POST request
                HttpWebRequest HttpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + Host + ":" + Port + "/json_rpc");
                HttpWebRequest.ContentType = "application/json-rpc";
                HttpWebRequest.Method = "POST";

                // Create a JSON request
                JObject JRequest = new JObject();
                if (Params != null) JRequest["params"] = Params;
                JRequest.Add(new JProperty("jsonrpc", "2.0"));
                JRequest.Add(new JProperty("id", "0"));
                JRequest.Add(new JProperty("method", Method));
                if (Password != null) JRequest.Add(new JProperty("password", Password));
                string Request = JRequest.ToString();

                if (Method != "getStatus" && TrtlBotSharp.logLevel >= 3) Console.WriteLine(Request);

                // Send bytes to server
                byte[] ByteArray = Encoding.UTF8.GetBytes(Request);
                HttpWebRequest.ContentLength = ByteArray.Length;
                Stream Stream = HttpWebRequest.GetRequestStream();
                Stream.Write(ByteArray, 0, ByteArray.Length);
                Stream.Close();

                // Receive reply from server
                WebResponse WebResponse = HttpWebRequest.GetResponse();
                StreamReader reader = new StreamReader(WebResponse.GetResponseStream(), Encoding.UTF8);

                // Get response
                Result = JObject.Parse(reader.ReadToEnd());
                if (Method != "getStatus" && TrtlBotSharp.logLevel >= 3) Console.WriteLine(Result.ToString());
                if (Result.ContainsKey("result")) Result = (JObject)Result["result"];

                // Dispose of pieces
                reader.Dispose();
                WebResponse.Dispose();
            }
            catch (Exception e)
            {
                TrtlBotSharp.Log(2, "TrtlBot", "Failed while sending request to host {0}: {1}", Host, e.Message);
            }
            return Result;
        }

        // Gets page source
        public static JObject GET(string Host)
        {
            JObject Result = new JObject();
            try
            {
                // Create a disposable web client
                using (WebClient client = new WebClient())
                {
                    // Get response
                    Result = JObject.Parse(client.DownloadString(Host));
                }
            }
            catch
            {
                TrtlBotSharp.Log(2, "TrtlBotSharp", "Failed while fetching data from host {0}", Host);
            }
            return Result;
        }
    }
}
