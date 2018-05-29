using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TrtlBotSharp
{
    partial class TrtlBotSharp
    {
        // Discord.Net Variables
        public static DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;

        // File Variables
        public static string
            configFile = "config.json",
            databaseFile = "users.db";

        // Operation Variables
        public static int
            logLevel = 1;

        // Permission Variables
        [JsonExtensionData]
        public static List<ulong>
            marketDisallowedServers = new List<ulong>
            {
                388915017187328002
            };

        // Bot Variables
        public static string
            botToken = "0",
            botPrefix = ".";
        public static int
            botMessageCache = 0;

        // Currency Variables
        public static string
            coinName = "TurtleCoin",
            coinSymbol = "TRTL",
            coinAddressPrefix = "TRTL";
        public static decimal
            coinUnits = 100;
        public static int
            coinAddressLength = 99;

        // Tipping Variables
        public static decimal
            tipFee = 10;
        public static int
            tipMixin = 3;
        public static string
            tipDefaultAddress = "",
            tipSuccessReact = "💸",
            tipFailedReact = "🆘",
            tipLowBalanceReact = "❌",
            tipJoinReact = "tip";
        public static List<string>
            tipAddresses = new List<string>();
        public static Dictionary<string, decimal>
            tipCustomReacts = new Dictionary<string, decimal>();

        // Faucet Variables
        public static string
            faucetHost = "https://faucet.trtl.me/",
            faucetEndpoint = "https://faucet.trtl.me/balance",
            faucetAddress = "TRTLv14M1Q9223QdWMmJyNeY8oMjXs5TGP9hDc3GJFsUVdXtaemn1mLKA25Hz9PLu89uvDafx9A93jW2i27E5Q3a7rn8P2fLuVA";

        // Market Variables
        public static string
            marketSource = "TradeOgre",
            marketEndpoint = "https://tradeogre.com/api/v1/ticker/BTC-TRTL",
            marketBTCEndpoint = "https://www.bitstamp.net/api/ticker/";

        // Daemon Variables
        public static string
            daemonHost = "127.0.0.1";
        public static int
            daemonPort = 11898;

        // Wallet Variables
        public static string
            walletHost = "127.0.0.1",
            walletRpcPassword = "password";
        public static int
            walletPort = 8070,
            walletUpdateDelay = 5000;
    }
}
