using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace TrtlBotSharp
{
    partial class TrtlBotSharp
    {
        // Initialization
        public static void Main(string[] args)
        {
            // Begin bot process in its own thread
            RunBotAsync();

            // Wait for keypress to exit
            Console.ReadKey();

            // Close the database connection
            CloseDatabase();
        }

        // Initiate bot
        public static bool Disconnected = false;
        public static async void RunBotAsync()
        {
            // Set message cache size
            DiscordSocketConfig SocketConfig = new DiscordSocketConfig();
            SocketConfig.MessageCacheSize = botMessageCache;

            // Load local files
            Log(0, "TrtlBot", "Loading config");
            await LoadConfig();
            Log(0, "TrtlBot", "Loading database");
            await LoadDatabase();

            // Populate API variables
            _client = new DiscordSocketClient(SocketConfig);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            // Add event handlers
            _client.Log += Log;
            _client.Ready += Ready;

            // Register commands and start bot
            Log(0, "TrtlBot", "Starting discord client");
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();

            // Set tip bot address
            Log(0, "TrtlBot", "Setting default address");
            await SetAddress();

            // Rest until a disconnect is detected
            Disconnected = false;
            while (!Disconnected) { }
        }

        private static bool Monitoring = false;
        private static Task Ready()
        {
            // Begin wallet monitoring once gateway reports as ready
            if (!Monitoring)
            {
                Log(0, "TrtlBot", "Starting wallet monitor");
                BeginMonitoring();
                Monitoring = true;
            }
            return Task.CompletedTask;
        }

        // Register commands within API
        private static async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += MessageReceivedAsync;
            _client.ReactionAdded += ReactionAddedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        // Log event handler
        private static Task Log(LogMessage arg)
        {
            // Ignore invalids messages
            if (arg.Message == null) return Task.CompletedTask;

            // Log message to console
            if (!arg.Message.Contains("UNKNOWN_DISPATCH"))
                Log(0, arg.Source, arg.Message);

            // Restart if disconnected
            if (arg.Message.Contains("Disconnected"))
                Log(0, "TrtlBot", "Restarting bot...");

            // Completed
            return Task.CompletedTask;
        }

        // Message received
        private static async Task MessageReceivedAsync(SocketMessage arg)
        {
            // Get message and create a context
            if (!(arg is SocketUserMessage Message)) return;
            SocketCommandContext Context = new SocketCommandContext(_client, Message);

            // Process commands
            int argPos = 0;
            if (Message.HasStringPrefix(botPrefix, ref argPos) ||
                Message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute command and log errors to console
                var Result = await _commands.ExecuteAsync(Context, argPos, _services);
                if (!Result.IsSuccess) Console.WriteLine(Result.ErrorReason);
            }
        }

        // Reaction added
        private static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> CacheableMessage, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            // Get reaction data
            IUserMessage Message = await CacheableMessage.GetOrDownloadAsync();

            // Ignore own reactions
            if (Reaction.UserId == _client.CurrentUser.Id)
                return;

            // Get emote name
            string Emote = "";
            if (Reaction.Emote.ToString().IndexOf(':') > -1)
                Emote = Reaction.Emote.ToString().Substring(Reaction.Emote.ToString().IndexOf(':'), Reaction.Emote.ToString().LastIndexOf(':'));
            else Emote = Reaction.Emote.ToString();

            // Check if reaction is a join reaction
            if (Emote == tipJoinReact)
            {
                // Check if message is a tip message
                if (!Message.Content.StartsWith(botPrefix + "tip")) return;

                // Check tip amount
                decimal Amount = Convert.ToDecimal(Message.Content.Split(' ')[1]);
                if (Amount * coinUnits < tipFee) return;

                // Check if user exists in user table
                if (!CheckUserExists(Reaction.UserId))
                {
                    await Reaction.User.Value.SendMessageAsync(string.Format("You must register a wallet before you can tip! Use {0}help if you need any help.", botPrefix));
                    return;
                }

                // Remove duplicate mentions
                List<ulong> Users = new List<ulong>();
                foreach (ulong User in Message.MentionedUserIds)
                    if (User != Reaction.UserId) Users.Add(User);
                Users = Users.Distinct().ToList();

                // Create a list of users that have wallets
                List<ulong> TippableUsers = new List<ulong>();
                foreach (ulong Id in Users)
                {
                    if (CheckUserExists(Id) && Id != Reaction.UserId)
                        TippableUsers.Add(Id);
                }

                // Check that there are users to tip
                if (TippableUsers.Count < 1) return;

                // Check that user has enough balance for the tip
                if (GetBalance(Reaction.UserId) < Convert.ToDecimal(Amount) * TippableUsers.Count + tipFee)
                {
                    await Reaction.User.Value.SendMessageAsync(string.Format("Your balance is too low! Amount + Fee = **{0:N}** {1}",
                        Convert.ToDecimal(Amount) * TippableUsers.Count + tipFee, coinSymbol));
                    await Message.AddReactionAsync(new Emoji(tipLowBalanceReact));
                    return;
                }

                // Check that there is at least one user with a registered wallet
                if (Tip(Reaction.UserId, TippableUsers, Convert.ToDecimal(Amount), Message as SocketMessage))
                    await Message.AddReactionAsync(new Emoji(tipSuccessReact));
            }

            // Custom reacts
            else if (tipCustomReacts.ContainsKey(Emote))
            {
                // Get custom react amount
                decimal Amount = tipCustomReacts[Emote];

                // Check if user exists in user table
                if (!CheckUserExists(Reaction.UserId))
                {
                    await Reaction.User.Value.SendMessageAsync(string.Format("You must register a wallet before you can tip! Use {0}help if you need any help.", botPrefix));
                    return;
                }

                // Check if user is trying to tip themself
                if (Message.Author.Id == Reaction.UserId)
                    return;

                // Check that recipient has registered a wallet
                if (!CheckUserExists(Message.Author.Id))
                {
                    // Add tip failed react
                    await (Message as SocketUserMessage).AddReactionAsync(new Emoji(tipFailedReact));

                    try
                    {
                        // Begin building a response
                        var Response = new EmbedBuilder();
                        Response.WithTitle(string.Format("{0} wants to tip you!", _client.GetUser(Reaction.UserId).Username));
                        Response.Description = string.Format("Register your wallet with `{0}registerwallet <your {1} address>` " +
                            "to get started!\nTo create a wallet head to https://turtlecoin.lol/wallet/\nExtra Help: http://docs.turtlecoin.lol/",
                            botPrefix, coinSymbol);

                        // Send reply
                        await Message.Author.SendMessageAsync("", false, Response);
                    }
                    catch { }
                    return;
                }

                // Check that user has enough balance for the tip
                if (GetBalance(Reaction.UserId) < Amount + tipFee)
                {
                    await Reaction.User.Value.SendMessageAsync(string.Format("Your balance is too low! Amount + Fee = **{0:N}** {1}",
                        Amount + tipFee, coinSymbol));
                    await Message.AddReactionAsync(new Emoji(tipLowBalanceReact));
                }

                // Tip has required arguments
                else if (Tip(Reaction.UserId, new List<ulong> { Message.Author.Id }, Amount, Message))
                {
                    // Send success react
                    await Message.AddReactionAsync(new Emoji(tipSuccessReact));
                }
            }
        }
    }
}
