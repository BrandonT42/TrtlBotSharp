using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    partial class TrtlBotSharp
    {
        // Loads config values from file
        public static async Task LoadConfig()
        {
            // Check if config file exists and create it if it doesn't
            if (!File.Exists(configFile)) await SaveConfig();
            else
            {
                // Load values
                JObject Config = JObject.Parse(File.ReadAllText(configFile));
                databaseFile = (string)Config["databaseFile"];
                logLevel = (int)Config["logLevel"];
                botToken = (string)Config["botToken"];
                botPrefix = (string)Config["botPrefix"];
                botMessageCache = (int)Config["botMessageCache"];
                coinName = (string)Config["coinName"];
                coinSymbol = (string)Config["coinSymbol"];
                coinUnits = (decimal)Config["coinUnits"];
                tipFee = (decimal)Config["tipFee"];
                tipMixin = (int)Config["tipMixin"];
                tipSuccessReact = (string)Config["tipSuccessReact"];
                tipFailedReact = (string)Config["tipFailedReact"];
                tipLowBalanceReact = (string)Config["tipLowBalanceReact"];
                tipJoinReact = (string)Config["tipJoinReact"];
                tipCustomReacts = Config["tipCustomReacts"].ToObject<Dictionary<string, decimal>>();
                faucetHost = (string)Config["faucetHost"];
                faucetEndpoint = (string)Config["faucetEndpoint"];
                faucetAddress = (string)Config["faucetAddress"];
                marketSource = (string)Config["marketSource"];
                marketEndpoint = (string)Config["marketEndpoint"];
                marketBTCEndpoint = (string)Config["marketBTCEndpoint"];
                marketDisallowedServers = Config["marketDisallowedServers"].ToObject<List<ulong>>();
                daemonHost = (string)Config["daemonHost"];
                daemonPort = (int)Config["daemonPort"];
                walletHost = (string)Config["walletHost"];
                walletPort = (int)Config["walletPort"];
                walletRpcPassword = (string)Config["walletRpcPassword"];
                walletUpdateDelay = (int)Config["walletUpdateDelay"];
            }
        }

        // Saves config values to file
        public static Task SaveConfig()
        {
            // Store values
            JObject Config = new JObject
            {
                ["databaseFile"] = databaseFile,
                ["logLevel"] = logLevel,
                ["botToken"] = botToken,
                ["botPrefix"] = botPrefix,
                ["botMessageCache"] = botMessageCache,
                ["coinName"] = coinName,
                ["coinSymbol"] = coinSymbol,
                ["coinUnits"] = coinUnits,
                ["tipFee"] = tipFee,
                ["tipMixin"] = tipMixin,
                ["tipSuccessReact"] = tipSuccessReact,
                ["tipFailedReact"] = tipFailedReact,
                ["tipLowBalanceReact"] = tipLowBalanceReact,
                ["tipJoinReact"] = tipJoinReact,
                ["tipCustomReacts"] = JToken.FromObject(tipCustomReacts),
                ["faucetHost"] = faucetHost,
                ["faucetEndpoint"] = faucetEndpoint,
                ["faucetAddress"] = faucetAddress,
                ["marketSource"] = marketSource,
                ["marketEndpoint"] = marketEndpoint,
                ["marketBTCEndpoint"] = marketBTCEndpoint,
                ["marketDisallowedServers"] = JToken.FromObject(marketDisallowedServers),
                ["daemonHost"] = daemonHost,
                ["daemonPort"] = daemonPort,
                ["walletHost"] = walletHost,
                ["walletPort"] = walletPort,
                ["walletRpcPassword"] = walletRpcPassword,
                ["walletUpdateDelay"] = walletUpdateDelay
            };

            // Flush to file
            File.WriteAllText(configFile, Config.ToString());

            // Completed
            return Task.CompletedTask;
        }
    }
}
