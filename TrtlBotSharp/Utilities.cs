using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    partial class TrtlBotSharp
    {
        // Gets coin supply
        public static decimal GetSupply()
        {
            // Get last block header from daemon
            JObject Result = Request.RPC(daemonHost, daemonPort, "getlastblockheader");
            if (Result.Count < 1 || Result.ContainsKey("error")) return 0;

            // Use last block hash to get last block from the network
            string LastBlockHash = (string)Result["block_header"]["hash"];
            Result = Request.RPC(daemonHost, daemonPort, "f_block_json", new JObject { ["hash"] = LastBlockHash });
            if (Result.Count < 1 || Result.ContainsKey("error")) return 0;

            // Return current supply
            return (decimal)Result["block"]["alreadyGeneratedCoins"] / coinUnits;
        }

        // Gets the bot's wallet address
        public static Task SetAddress()
        {
            // Get address list from wallet
            JObject Result = Request.RPC(walletHost, walletPort, "getAddresses", null, walletRpcPassword);
            if (Result.Count < 1 || Result.ContainsKey("error")) tipDefaultAddress = "";

            else
            {
                // Set address list
                tipAddresses = new List<string>();
                foreach (string Address in (JArray)Result["addresses"])
                    tipAddresses.Add(Address);

                // Retrieve first address
                tipDefaultAddress = (string)Result["addresses"][0];
            }

            // Completed
            return Task.CompletedTask;
        }

        // Formats hashrate into a readable format
        public static string FormatHashrate(decimal Hashrate)
        {
            int i = 0;
            string[] Units = { " H/s", " KH/s", " MH/s", " GH/s", " TH/s", " PH/s" };
            while (Hashrate > 1000)
            {
                Hashrate /= 1000;
                i++;
            }
            return string.Format("{0:N} {1}", Hashrate, Units[i]);
        }

        // Prettifies a message desription
        public static string Prettify(string Input)
        {
            // Split lines
            string[] Lines = Input.Split('\n');

            // Loop through lines to get largest header size
            int HeaderSize = 0;
            foreach (string Line in Lines)
            {
                // Check if line contains tab character
                if (Line.IndexOf('\t') > -1)
                {
                    // Split at tab character
                    string[] Section = Line.Split('\t');

                    // Compare header to last largest size
                    if (Section[0].Length > HeaderSize)
                        HeaderSize = Section[0].Length;
                }
            }

            // Add spaces until all headers are uniform
            string Output = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                // Check if line contains tab character
                if (Lines[i].IndexOf('\t') > -1)
                {
                    // Split at tab character
                    string[] Section = Lines[i].Split('\t');

                    // Compare header to largest size and add remaining characters
                    while (Section[0].Length < HeaderSize)
                        Section[0] += " ";

                    // Set line to formatted line
                    Lines[i] = Section[0] + " " + Section[1];
                }

                // Add line to output
                Output += Lines[i];
                if (i < Lines.Length - 1) Output += "\n";
            }

            // Return prettified output
            return Output;
        }

        // Generates a payment id
        public static string GeneratePaymentId(string address)
        {
            // Create a unique randomizer
            Random Random = new Random((address + botToken).GetHashCode());

            // Create byte variables
            byte[] Buffer = new byte[32];
            Random.NextBytes(Buffer);

            // Return resulting string
            return String.Concat(Buffer.Select(x => x.ToString("X2")).ToArray());
        }

        // Verifies an address is the correct format
        public static bool VerifyAddress(string Address)
        {
            // Check address length
            if (Address.Length != coinAddressLength) return false;

            // Check address prefix
            if (Address.Substring(0, coinAddressPrefix.Length).ToLower() != coinAddressPrefix.ToLower()) return false;

            // Verified as valid
            return true;
        }

        // Returns the minimum coin value
        public static decimal Minimum
        {
            get { return 1 / coinUnits; }
        }

        // Log to command line
        public static void Log(int LogLevel, string Source, string Message, params object[] Objects)
        {
            // Formate and write to command line
            if (Message != null && logLevel >= LogLevel)
                    Console.WriteLine("{0} {1}\t{2}", DateTime.Now, Source, string.Format(Message, Objects));
        }
    }
}
