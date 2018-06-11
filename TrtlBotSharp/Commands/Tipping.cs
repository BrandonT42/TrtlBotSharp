using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("registerwallet")]
        public async Task RegisterWalletAsync(string Address, [Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Check that user hasn't already registered an address
            if (TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
                await Context.Message.Author.SendMessageAsync(string.Format("You have already registered an address, use {0}updatewallet if you'd like to update it", TrtlBotSharp.botPrefix));

            // Check address validity
            else if (!TrtlBotSharp.VerifyAddress(Address))
                await Context.Message.Author.SendMessageAsync(string.Format("Address is not a valid {0} address!", TrtlBotSharp.coinName));

            // Check that address isn't in use by another user
            else if (TrtlBotSharp.CheckAddressExists(Address))
                await Context.Message.Author.SendMessageAsync("Address is in use by another user");

            // Passed checks
            else
            {
                // Register wallet into database
                string PaymentId = TrtlBotSharp.RegisterWallet(Context.Message.Author.Id, Address);

                // Begin building a response
                var Response = new EmbedBuilder();
                Response.WithTitle("Successfully registered your wallet!");
                Response.Description = string.Format("Deposit {0} to start tipping!\n\n" +
                    "Address:\n**{1}**\n\nPayment ID:\n**{2}**", TrtlBotSharp.coinSymbol, TrtlBotSharp.tipDefaultAddress, PaymentId);

                // Send reply
                await Context.Message.Author.SendMessageAsync("", false, Response);
            }
        }

        [Command("updatewallet")]
        public async Task UpdateWalletAsync(string Address, [Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Check that user has registered an address, register it if not.
            if (!TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
            {
                await RegisterWalletAsync(Address, Remainder);
                return;
            }

            // Check address validity
            else if (!TrtlBotSharp.VerifyAddress(Address))
                await Context.Message.Author.SendMessageAsync(string.Format("Address is not a valid {0} address!", TrtlBotSharp.coinName));

            // Check that address isn't in use by another user
            else if (TrtlBotSharp.CheckAddressExists(Address))
                await Context.Message.Author.SendMessageAsync("Address is in use by another user");

            // Passed checks
            else
            {
                // Update address in database
                TrtlBotSharp.UpdateWallet(Context.Message.Author.Id, Address);

                // Reply with success
                await Context.Message.Author.SendMessageAsync("Successfully updated your wallet");
            }
        }

        [Command("redirecttips")]
        public async Task RedirectTipsAsync([Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Check that user has registered an address
            if (!TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
                await Context.Message.Author.SendMessageAsync(string.Format("You must register a wallet before you can recieve tips! Use {0}help if you need any help.", TrtlBotSharp.botPrefix));

            // User is registered
            else
            {
                // Check if user is redirecting tips
                bool Redirect = TrtlBotSharp.GetRedirect(Context.Message.Author.Id);

                // Set new value
                if (Redirect) Redirect = false;
                else Redirect = true;

                // Set redirect preference
                TrtlBotSharp.SetRedirect(Context.Message.Author.Id, Redirect);

                // Send reply
                if (Redirect) await Context.Message.Author.SendMessageAsync("**Tip redirect preference changed**\nTips you receive will now go to your tip jar");
                else await Context.Message.Author.SendMessageAsync("**Tip redirect preference changed**\nTips you receive will now go to your registered wallet");
            }
        }
        [Command("redirecttips")]
        public async Task RedirectTipsAsync(bool Redirect, [Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Check that user has registered an address
            if (!TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
                await Context.Message.Author.SendMessageAsync(string.Format("You must register a wallet before you can recieve tips! Use {0}help if you need any help.", TrtlBotSharp.botPrefix));

            // User is registered
            else
            {
                // Set redirect preference
                TrtlBotSharp.SetRedirect(Context.Message.Author.Id, Redirect);

                // Send reply
                if (Redirect) await Context.Message.Author.SendMessageAsync("**Tip redirect preference changed**\nTips you receive will now go to your tip jar");
                else await Context.Message.Author.SendMessageAsync("**Tip redirect preference changed**\nTips you receive will now go to your registered wallet");
            }
        }

        [Command("wallet")] // Get own wallet
        public async Task WalletAsync([Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Try to grab address from the database
            string Address = "";
            if (TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
                Address = TrtlBotSharp.GetAddress(Context.Message.Author.Id);

            // Check if result is empty
            if (string.IsNullOrEmpty(Address))
                await Context.Message.Author.SendMessageAsync(string.Format("You haven't registered a wallet! Use {0}help if you need any help.",
                    TrtlBotSharp.botPrefix));

            // Check if user is requesting their own wallet
            else await Context.Message.Author.SendMessageAsync(string.Format("**Your wallet:**```{0}```", Address));
        }
        [Command("wallet")] // Get by mention
        public async Task WalletAsync(SocketUser User, [Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Try to grab address from the database
            string Address = "";
            if (TrtlBotSharp.CheckUserExists(User.Id))
                Address = TrtlBotSharp.GetAddress(User.Id);

            // Check if result is empty
            if (string.IsNullOrEmpty(Address))
                await Context.Message.Author.SendMessageAsync(string.Format("{0} hasn't registered a wallet!", User.Username));

            // Check if user is requesting their own wallet
            else if (User == null || Context.Message.Author.Id == User.Id)
                await Context.Message.Author.SendMessageAsync(string.Format("**Your wallet:**```{0}```", Address));

            // User is requesting someone else's wallet
            else await Context.Message.Author.SendMessageAsync(string.Format("**{0}'s wallet:**```{1}```", User.Username, Address));
        }
        [Command("wallet")] // Get by uid
        public async Task WalletAsync(ulong UID, [Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Try to grab address from the database
            string Address = "";
            if (TrtlBotSharp.CheckUserExists(UID))
                Address = TrtlBotSharp.GetAddress(UID);

            // Get requested user
            string Username = "";
            SocketUser User = Context.Client.GetUser(UID);
            if (User != null) Username = User.Username;
            if (string.IsNullOrEmpty(Username)) Username = UID.ToString();

            // Check if result is empty
            if (string.IsNullOrEmpty(Address))
                await Context.Message.Author.SendMessageAsync(string.Format("{0} hasn't registered a wallet!", Username));

            // Check if user is requesting their own wallet
            else if (Context.Message.Author.Id == UID)
                await Context.Message.Author.SendMessageAsync(string.Format("**Your wallet:**```{0}```", Address));

            // User is requesting someone else's wallet
            else await Context.Message.Author.SendMessageAsync(string.Format("**{0}'s wallet:**```{1}```", Username, Address));
        }

        [Command("deposit")]
        public async Task DepositAsync([Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Check if user exists in user table
            if (!TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
                await Context.Message.Author.SendMessageAsync(string.Format("You must register a wallet before you can deposit! Use {0}help if you need any help.",
                    TrtlBotSharp.botPrefix));

            // Send reply
            else await Context.Message.Author.SendMessageAsync(string.Format("**Deposit {0} to start tipping!**```Address:\n{1}\n\nPayment ID:\n{2}```", 
                TrtlBotSharp.coinSymbol, TrtlBotSharp.tipDefaultAddress, TrtlBotSharp.GetPaymentId(Context.Message.Author.Id)));
        }

        [Command("withdraw")]
        public async Task WithdrawAsync(string Amount, [Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Check if user exists in user table
            if (!TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
            {
                await Context.Message.Author.SendMessageAsync(string.Format("You must register a wallet before you can withdraw! Use {0}help if you need any help.", TrtlBotSharp.botPrefix));
                return;
            }

            // Check that amount is over the minimum fee
            else if (Convert.ToDecimal(Amount) < TrtlBotSharp.Minimum)//TrtlBotSharp.Fee)
            {
                await ReplyAsync(string.Format("Amount must be at least {0:N} {1}", TrtlBotSharp.Minimum/*Fee*/, TrtlBotSharp.coinSymbol));
                return;
            }

            // Check if user has enough balance
            else if (TrtlBotSharp.GetBalance(Context.Message.Author.Id) < Convert.ToDecimal(Amount) + TrtlBotSharp.tipFee)
            {
                await Context.Message.Author.SendMessageAsync(string.Format("Your balance is too low! Amount + Fee = **{0:N}** {1}",
                    Convert.ToDecimal(Amount) + TrtlBotSharp.tipFee, TrtlBotSharp.coinSymbol));
                await Context.Message.AddReactionAsync(new Emoji(TrtlBotSharp.tipLowBalanceReact));
            }

            // Send withdrawal
            else if (TrtlBotSharp.Tip(Context.Message.Author.Id, TrtlBotSharp.GetAddress(Context.Message.Author.Id), Convert.ToDecimal(Amount)))
            {
                // Send success react
                await Context.Message.AddReactionAsync(new Emoji(TrtlBotSharp.tipSuccessReact));
            }
        }

        [Command("balance")]
        public async Task BalanceAsync([Remainder]string Remainder = "")
        {
            // Delete original message
            try { await Context.Message.DeleteAsync(); }
            catch { }

            // Check if user exists in user table
            if (!TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
                await Context.Message.Author.SendMessageAsync(string.Format("You must register a wallet before you can check your tip jar balance! Use {0}help if you need any help.",
                    TrtlBotSharp.botPrefix));

            // Send reply with balance
            else
            {
                // Get balance
                decimal Balance = TrtlBotSharp.GetBalance(Context.Message.Author.Id);

                // Send reply
                await Context.Message.Author.SendMessageAsync(string.Format("You have **{0:N}** {1} in your tip jar", Balance, TrtlBotSharp.coinSymbol));
            }
        }

        [Command("tip")]
        public async Task TipAsync(string Amount, [Remainder]string Remainder = "")
        {
            // Check if user exists in user table
            if (!TrtlBotSharp.CheckUserExists(Context.Message.Author.Id))
            {
                await Context.Message.Author.SendMessageAsync(string.Format("You must register a wallet before you can tip! Use {0}help if you need any help.", TrtlBotSharp.botPrefix));
                return;
            }

            // Check that amount is over the minimum fee
            if (Convert.ToDecimal(Amount) < TrtlBotSharp.Minimum)//TrtlBotSharp.Fee)
            {
                await ReplyAsync(string.Format("Amount must be at least {0:N} {1}", TrtlBotSharp.Minimum/*Fee*/, TrtlBotSharp.coinSymbol));
                return;
            }

            // Check if an address is specified instead of mentioned users
            string Address = "";
            if (Remainder.StartsWith(TrtlBotSharp.coinAddressPrefix) && Remainder.Length == TrtlBotSharp.coinAddressLength)
                Address = Remainder.Substring(0, 99);

            // Check that there is at least one mentioned user
            if (Address == "" && Context.Message.MentionedUsers.Count < 1) return;

            // Remove duplicate mentions
            List<ulong> Users = new List<ulong>();
            foreach (SocketUser MentionedUser in Context.Message.MentionedUsers)
                Users.Add(MentionedUser.Id);
            Users = Users.Distinct().ToList();

            // Create a list of users that have wallets
            List<ulong> TippableUsers = new List<ulong>();
            foreach (ulong Id in Users)
            {
                if (TrtlBotSharp.CheckUserExists(Id) && Id != Context.Message.Author.Id)
                    TippableUsers.Add(Id);
            }

            // Check that user has enough balance for the tip
            if (Address == "" && TrtlBotSharp.GetBalance(Context.Message.Author.Id) < Convert.ToDecimal(Amount) * TippableUsers.Count + TrtlBotSharp.tipFee)
            {
                await Context.Message.Author.SendMessageAsync(string.Format("Your balance is too low! Amount + Fee = **{0:N}** {1}",
                    Convert.ToDecimal(Amount) * TippableUsers.Count + TrtlBotSharp.tipFee, TrtlBotSharp.coinSymbol));
                await Context.Message.AddReactionAsync(new Emoji(TrtlBotSharp.tipLowBalanceReact));
            }
            else if (TrtlBotSharp.GetBalance(Context.Message.Author.Id) < Convert.ToDecimal(Amount) + TrtlBotSharp.tipFee)
            {
                await Context.Message.Author.SendMessageAsync(string.Format("Your balance is too low! Amount + Fee = **{0:N}** {1}",
                    Convert.ToDecimal(Amount) + TrtlBotSharp.tipFee, TrtlBotSharp.coinSymbol));
                await Context.Message.AddReactionAsync(new Emoji(TrtlBotSharp.tipLowBalanceReact));
            }

            // Tip has required arguments
            else
            {
                if (Address == "")
                {
                    // Send a failed react if a user isn't found
                    bool FailReactAdded = false;
                    foreach (ulong User in Users)
                        if (User != Context.Message.Author.Id && !TippableUsers.Contains(User))
                        {
                            if (!FailReactAdded)
                            {
                                await Context.Message.AddReactionAsync(new Emoji(TrtlBotSharp.tipFailedReact));
                                FailReactAdded = true;
                            }

                            try {
                                // Begin building a response
                                var Response = new EmbedBuilder();
                                Response.WithTitle(string.Format("{0} wants to tip you!", Context.Message.Author.Username));
                                Response.Description = string.Format("Register your wallet with with `{0}registerwallet <your {1} address>` " +
                                    "to get started!\nTo create a wallet head to https://turtlecoin.lol/wallet/\nExtra Help: http://docs.turtlecoin.lol/",
                                    TrtlBotSharp.botPrefix, TrtlBotSharp.coinSymbol);

                                // Send reply
                                await Context.Client.GetUser(User).SendMessageAsync("", false, Response);
                            }
                            catch { }
                        }

                    // Check that there is at least one user with a registered wallet
                    if (TippableUsers.Count > 0 && TrtlBotSharp.Tip(Context.Message.Author.Id, TippableUsers, Convert.ToDecimal(Amount), Context.Message))
                    {
                        // Send success react
                        await Context.Message.AddReactionAsync(new Emoji(TrtlBotSharp.tipSuccessReact));
                    }
                }
                else if (TrtlBotSharp.Tip(Context.Message.Author.Id, Address, Convert.ToDecimal(Amount)))
                {
                    // Send success react
                    await Context.Message.AddReactionAsync(new Emoji(TrtlBotSharp.tipSuccessReact));
                }
            }
        }
    }
}
