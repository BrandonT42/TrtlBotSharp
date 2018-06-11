using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task HelpAsync([Remainder]string Remainder = "")
        {
            // Begin building a response
            EmbedBuilder Response = new EmbedBuilder();
            Response.WithTitle("Help");
            string Output = "";

            // Requesting additional help
            if (Remainder.ToLower() == "faucet")
            {
                Response.Title += string.Format(" - {0}faucet", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}faucet", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", "Gives faucet information, including the donation address, a link to the faucet, and how much it has left");
            }
            else if (Remainder.ToLower() == "hashrate")
            {
                Response.Title += string.Format(" - {0}hashrate", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}hashrate", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", "Gives the current network hashrate");
            }
            else if (Remainder.ToLower() == "difficulty")
            {
                Response.Title += string.Format(" - {0}difficulty", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}difficulty", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", "Gives the current network difficulty");
            }
            else if (Remainder.ToLower() == "height")
            {
                Response.Title += string.Format(" - {0}height", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}height", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", "Gives the current network height");
            }
            else if (Remainder.ToLower() == "supply")
            {
                Response.Title += string.Format(" - {0}supply", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}supply", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", string.Format("Gives the total circulating supply of {0}", TrtlBotSharp.coinSymbol));
            }
            else if (Remainder.ToLower() == "registerwallet")
            {
                Response.Title += string.Format(" - {0}registerwallet", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}registerwallet <{1} Address>", TrtlBotSharp.botPrefix, TrtlBotSharp.coinSymbol));
                Response.AddField("Description:", "Registers your address with the bot so you may send and recieve tips");
            }
            else if (Remainder.ToLower() == "updatewallet")
            {
                Response.Title += string.Format(" - {0}updatewallet", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}updatewallet <{1} Address>", TrtlBotSharp.botPrefix, TrtlBotSharp.coinSymbol));
                Response.AddField("Description:", "Updates your registered wallet to a new address");
            }
            else if (Remainder.ToLower() == "wallet")
            {
                Response.Title += string.Format(" - {0}uwallet", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}wallet\n{0}wallet <{1} Address>", TrtlBotSharp.botPrefix, TrtlBotSharp.coinSymbol));
                Response.AddField("Description:", "Gets a specified user's registered wallet address, or your own if no address is specified");
            }
            else if (Remainder.ToLower() == "deposit")
            {
                Response.Title += string.Format(" - {0}deposit", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}deposit", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", string.Format("DMs you with your deposit information, including the address to send to, " +
                    "and the payment ID you **must** use when sending {0}", TrtlBotSharp.coinSymbol));
            }
            else if (Remainder.ToLower() == "withdraw")
            {
                Response.Title += string.Format(" - {0}withdraw", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}withdraw <Amount of {1}>", TrtlBotSharp.botPrefix, TrtlBotSharp.coinSymbol));
                Response.AddField("Description:", "Withdraws a specified amount from your tip jar into your registered wallet");
            }
            else if (Remainder.ToLower() == "balance")
            {
                Response.Title += string.Format(" - {0}balance", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}balance", TrtlBotSharp.botPrefix, TrtlBotSharp.coinSymbol));
                Response.AddField("Description:", "Gets your current tip jar balance");
            }
            else if (Remainder.ToLower() == "tip")
            {
                Response.Title += string.Format(" - {0}tip", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}tip <Amount of {1}> @Users1 @User2...\n{0}tip <Amount of {1}> <{1} Address>", 
                    TrtlBotSharp.botPrefix, TrtlBotSharp.coinSymbol));
                Response.AddField("Description:", string.Format("Sends a tip of a specified amount to one or more users *or* a specified {0} address", 
                    TrtlBotSharp.coinSymbol));
            }
            else if (Remainder.ToLower() == "redirecttips")
            {
                Response.Title += string.Format(" - {0}redirecttips", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}redirecttips\n{0}redirecttips <True or False>", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", "Sets whether you'd like to have tips sent to you to go directly to your registered wallet " +
                    "(default) or redirected into your tip jar balance");
            }

            else if (Remainder.ToLower() == "price" && (Context.Guild == null || !TrtlBotSharp.marketDisallowedServers.Contains(Context.Guild.Id)))
            {
                Response.Title += string.Format(" - {0}price", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}price", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", string.Format("Gives the current price of {0} in USD and BTC", TrtlBotSharp.coinSymbol));
            }
            else if (Remainder.ToLower() == "mcap" && (Context.Guild == null || !TrtlBotSharp.marketDisallowedServers.Contains(Context.Guild.Id)))
            {
                Response.Title += string.Format(" - {0}mcap", TrtlBotSharp.botPrefix);
                Response.AddField("Usage:", string.Format("{0}mcap", TrtlBotSharp.botPrefix));
                Response.AddField("Description:", string.Format("Gives {0}'s current market capitalization", TrtlBotSharp.coinSymbol));
            }

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
                Output += "  withdraw\tWithdraws a specified amount from your tip jar into your registered wallet\n";
                Output += "  balance\tGives your current tip jar balance\n";
                Output += "  tip\tTips one or more users a specified amount\n";
                Output += "  redirecttips\tSets whether you'd like tips sent directly to your wallet or redirected back into your tip jar";
                Output = string.Format("```" + TrtlBotSharp.Prettify(Output) + "```**Note:** You can use *{0}help <Name of Command>* for " +
                    "additional help with any command", TrtlBotSharp.botPrefix);
                Response.WithDescription(Output);
                Response.WithTitle("Available Commands:");
            }

            // Send reply
            await ReplyAsync("", false, Response);
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
