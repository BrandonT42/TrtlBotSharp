using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync([Remainder]string Remainder = "")
        {
            // Begin building a response
            string Output = "";

            // Requesting additional help
            if (Remainder.ToLower() == "faucet")
            {
                Output = "**Command:**```";
                Output += "Usage:\n";
                Output += string.Format("{0}faucet\n", TrtlBotSharp.botPrefix);
                Output += "\n";
                Output += "Description:\n";
                Output += "Gives faucet information including the donation address and a link to the faucet";
                Output += "```";
            }
            else if (Remainder.ToLower() == "hashrate") { }
            else if (Remainder.ToLower() == "difficulty") { }
            else if (Remainder.ToLower() == "height") { }
            else if (Remainder.ToLower() == "supply") { }
            else if (Remainder.ToLower() == "registerwallet") { }
            else if (Remainder.ToLower() == "updatewallet") { }
            else if (Remainder.ToLower() == "wallet") { }
            else if (Remainder.ToLower() == "deposit") { }
            else if (Remainder.ToLower() == "balance") { }
            else if (Remainder.ToLower() == "tip")
            {
                Output = "**Command:**```";
                Output += "Usage:\n";
                Output += string.Format("{0}tip 12.34 @User1 @User2\n", TrtlBotSharp.botPrefix);
                Output += string.Format("{0}tip 12.34 TRTLv1...\n", TrtlBotSharp.botPrefix);
                Output += "\n";
                Output += "Description:\n";
                Output += "Tips one or more users a specified amount";
                Output += "```";
            }

            else if (Remainder.ToLower() == "price" && (Context.Guild == null || !TrtlBotSharp.marketDisallowedServers.Contains(Context.Guild.Id))) { }
            else if (Remainder.ToLower() == "mcap" && (Context.Guild == null || !TrtlBotSharp.marketDisallowedServers.Contains(Context.Guild.Id))) { }

            // No requested command
            else
            {
                Output += "Informational:\n";
                Output += "  help\tLists all available commands\n";
                Output += "  faucet\tGives faucet information\n";
                Output += "Network:\n";
                Output += "  hashrate\tGives current network hashrate\n";
                Output += "  difficulty\tGives current network difficulty\n";
                Output += "  height\tGives current network height\n";
                Output += "  supply\tGives current circulating supply\n";
                if (Context.Guild == null || !TrtlBotSharp.marketDisallowedServers.Contains(Context.Guild.Id))
                {
                    Output += "Market:\n";
                    Output += "  price\tGives current price\n";
                    Output += "  mcap\tGives current global marketcap\n";
                }
                Output += "Tipping:\n";
                Output += "  registerwallet\tRegisters your wallet with the tip bot\n";
                Output += "  updatewallet\tUpdates your registered wallet\n";
                Output += "  wallet\tGives the wallet address for a specified user or your own address if no user is specified\n";
                Output += "  deposit\tGives information on how to deposit into your tipping balance\n";
                Output += "  withdraw\tWithdraws a specified amount from your tip jar into your main wallet\n";
                Output += "  balance\tGives your current tipping balance\n";
                Output += "  tip\tTips one or more users a specified amount";
                Output = "**Commands:**\n```" + TrtlBotSharp.Prettify(Output) + "```";
            }

            // Send reply
            await ReplyAsync(Output);
        }

        [Command("faucet")]
        public async Task FaucetAsync([Remainder]string Remainder = "")
        {
            // Get faucet balance
            JObject FaucetBalance = Request.GET(TrtlBotSharp.faucetEndpoint);
            if (FaucetBalance.Count < 1)
            {
                await ReplyAsync("Failed to connect to faucet");
                return;
            }

            // Begin building a response
            var Response = new EmbedBuilder();
            Response.WithTitle(string.Format("This faucet has {0:N} {1} left", (decimal)FaucetBalance["available"], TrtlBotSharp.coinSymbol));
            Response.WithUrl(TrtlBotSharp.faucetHost);
            Response.Description = "```Donations:\n" + TrtlBotSharp.faucetAddress + "```\n";

            // Send reply
            await ReplyAsync("", false, Response);
        }
    }
}
