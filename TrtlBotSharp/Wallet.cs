using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // List of confirmed transactions
        public static Dictionary<string, JObject> ConfirmedTransactions = new Dictionary<string, JObject>();

        // List of unconfirmed transactions
        public static Dictionary<string, JObject> UnconfirmedTransactions = new Dictionary<string, JObject>();

        // Sends a tip to a list of users
        public static bool Tip(ulong Sender, List<ulong> Recipients, decimal Amount, IMessage Message)
        {
            // Create transfer list
            JArray Transfers = new JArray();

            // Loop through recipients and add them to transfer list
            foreach (ulong UID in Recipients)
            {
                // Get wallet address
                string Address = "";
                if (!GetRedirect(UID)) Address = GetAddress(UID);
                else Address = tipDefaultAddress;

                // Add to transfer list
                Transfers.Add(new JObject()
                {
                    ["address"] = Address,
                    ["amount"] = Convert.ToInt64(Amount * coinUnits)
                });
            }

            // Calculate total amount for logging
            decimal TotalAmount = Amount * Recipients.Count;

            // Send transaction and update database if successful
            if (SendTransaction(GetPaymentId(Sender), Transfers, TotalAmount, out string TransactionHash))
            {
                // Calculate new balance
                decimal Balance = GetBalance(Sender) - (TotalAmount + tipFee);

                // Update balance in database
                SetBalance(Sender, Balance);

                // Update recipients
                foreach (ulong UID in Recipients)
                {
                    // Update stats
                    UserStats(Sender, UID, Amount);

                    // Begin building a message
                    var ReplyEmbed = new EmbedBuilder();
                    ReplyEmbed.WithTitle("Tip received!");

                    // Update redirected funds
                    if (GetRedirect(UID))
                    {
                        // Calculate new balance
                        decimal NewBalance = GetBalance(UID) + Amount;

                        // Add to pending tips
                        AddPending(TransactionHash, GetPaymentId(UID), Amount);

                        // Set response message
                        ReplyEmbed.Description = string.Format("You recieved a tip of **{0:N}** {1} from {2}, since you are redirecting tips back to " +
                            "your tip jar, this will come through as a deposit once confirmed.\nTX: **{3}**", Amount, coinSymbol,
                            _client.GetUser(Sender).Username, TransactionHash);
                    }
                    else ReplyEmbed.Description = string.Format("You recieved a tip of **{0:N}** {1} from {2}!\nTX: **{3}**", Amount, coinSymbol,
                        _client.GetUser(Sender).Username, TransactionHash);

                    // Send message
                    _client.GetUser(UID).SendMessageAsync("", false, ReplyEmbed);
                }

                // Begin building a message
                var Response = new EmbedBuilder();
                Response.WithTitle("Tip sent!");
                if (Recipients.Count == 1)
                    Response.Description = string.Format("You sent a tip of **{0:N}** {1} to 1 user\nNew balance: **{2:N}** {1}\nTX: **{3}**",
                        Amount, coinSymbol, Balance, TransactionHash);
                else Response.Description = string.Format("You sent a tip of **{0:N}** {1} to {2} users\nNew balance: **{3:N}** {1}\nTX: **{4}**",
                        Amount, coinSymbol, Recipients.Count, Balance, TransactionHash);

                // Send message
                _client.GetUser(Sender).SendMessageAsync("", false, Response);

                // Update global stats
                ulong ServerId = 0;
                ulong ChannelId = 0;
                if (Message.Channel != null)
                {
                    ChannelId = Message.Channel.Id;
                    if ((Message.Channel as SocketGuildChannel).Guild != null)
                        ServerId = (Message.Channel as SocketGuildChannel).Guild.Id;
                }
                GlobalStats("OUT", ServerId, ChannelId, Sender, Amount, Recipients.Count);

                // Return as successful
                return true;
            }
            else return false;
        }

        // Sends a tip to a specified address
        public static bool Tip(ulong Sender, string Recipient, decimal Amount)
        {
            // Create transfer list
            JArray Transfers = new JArray();
            Transfers.Add(new JObject
                {
                    ["address"] = Recipient,
                    ["amount"] = Convert.ToInt64(Amount * coinUnits)
                }
            );

            // Calculate total amount for logging
            decimal TotalAmount = Amount;

            // Send transaction
            if (SendTransaction(GetPaymentId(Sender), Transfers, TotalAmount, out string TransactionHash))
            {
                // Calculate new balance
                decimal Balance = GetBalance(Sender) - (TotalAmount + tipFee);

                // Update balance in database
                SetBalance(Sender, Balance);

                // Begin building a message
                var Response = new EmbedBuilder();
                if (GetAddress(Sender) == Recipient)
                {
                    Response.WithTitle("Withdrawal sent!");
                    Response.Description = string.Format("You withdrew **{0:N}** {1} to {2}\nNew balance: **{3:N}** {1}\nTX: **{4}**",
                        Amount, coinSymbol, Recipient, Balance, TransactionHash);
                }
                else
                {
                    Response.WithTitle("Tip sent!");
                    Response.Description = string.Format("You sent a tip of **{0:N}** {1} to {2}\nNew balance: **{3:N}** {1}\nTX: **{4}**",
                        Amount, coinSymbol, Recipient, Balance, TransactionHash);
                }

                // Send message
                _client.GetUser(Sender).SendMessageAsync("", false, Response);

                // Return as successful
                return true;
            }
            else return false;
        }

        // Sends a transaction to the wallet server
        public static bool SendTransaction(string PaymentId, JArray Transfers, decimal TotalAmount, out string TransactionHash)
        {
            // Log transaction to console
            Log(1, "Wallet", "Sending transaction to {0} recipients using payment id {1}", Transfers.Count, PaymentId);

            // Build a transaction object
            JObject Transaction = new JObject()
            {
                ["paymentId"] = PaymentId,
                ["fee"] = Convert.ToInt64(tipFee * coinUnits),
                ["anonymity"] = tipMixin,
                ["transfers"] = Transfers,
                ["changeAddress"] = tipDefaultAddress
            };

            // Send transaction
            JObject Result = Request.RPC(walletHost, walletPort, "sendTransaction", Transaction, walletRpcPassword);
            if (Result.ContainsKey("transactionHash"))
            {
                // Log transaction
                Int32 Timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                LogTransaction(Timestamp.ToString(), "OUT", (string)Result["transactionHash"], PaymentId, TotalAmount);

                // Set transaction hash
                TransactionHash = (string)Result["transactionHash"];

                // Return as successful
                return true;
            }
            else
            {
                // Set transaction hash
                TransactionHash = "";

                // Return as unsuccessful
                return false;
            }
        }

        // Watches the wallet for incoming data
        public static async void BeginMonitoring()
        {
            // Get wallet status
            JObject Result = Request.RPC(walletHost, walletPort, "getStatus", null, walletRpcPassword);
            if (Result.Count > 0 && Result.ContainsKey("error")) return;

            // Get last sync height
            int SyncHeight = GetSyncHeight();
            Log(0, "Wallet", "Starting sync, {0:N0} blocks behind", ((int)Result["blockCount"] - SyncHeight));

            // Loop while connected
            while (Result.Count > 0 && !Result.ContainsKey("error"))
            {
                // Do loop
                try
                {
                    // Update wallet status
                    Result = Request.RPC(walletHost, walletPort, "getStatus", null, walletRpcPassword);
                    if (Result.Count < 1 || !Result.HasValues || Result.ContainsKey("error"))
                        continue;

                    // Get current height
                    int CurrentHeight = (int)Result["blockCount"];

                    // Scan for new transactions if sync height falls behind
                    while (SyncHeight < CurrentHeight)
                    {
                        // Log that syncing has started
                        Log(1, "Wallet", "Scanning for deposits from height: {0:N0}", SyncHeight);

                        // Set sync chunk size
                        int SyncSize = 1000;
                        if (CurrentHeight - SyncHeight < SyncSize)
                            SyncSize = CurrentHeight - SyncHeight;

                        // Get transaction list
                        JObject Transactions = Request.RPC(walletHost, walletPort, "getTransactions",
                            new JObject { ["firstBlockIndex"] = SyncHeight, ["blockCount"] = SyncSize }, walletRpcPassword);

                        // Loop through transaction data
                        if (Transactions.Count > 0 && !Transactions.ContainsKey("error"))
                        {
                            foreach (JObject Item in Transactions["items"])
                            {
                                foreach (JObject Transaction in Item["transactions"])
                                {
                                    // Check for a payment ID
                                    if ((string)Transaction["paymentId"] == "")
                                        continue;

                                    // Check if transaction is already logged
                                    else if (ConfirmedTransactions.ContainsKey((string)Transaction["transactionHash"]) ||
                                        UnconfirmedTransactions.ContainsKey((string)Transaction["transactionHash"]))
                                        continue;

                                    // Check if transaction is unlocked
                                    else if ((int)Transaction["unlockTime"] <= 3)
                                        ConfirmedTransactions.Add((string)Transaction["transactionHash"], Transaction);

                                    // Check if transactions is locked
                                    else if ((int)Transaction["unlockTime"] > 3)
                                        UnconfirmedTransactions.Add((string)Transaction["transationHash"], Transaction);
                                }
                            }
                        }

                        // Update sync height
                        SyncHeight = SyncHeight + SyncSize;
                        SetSyncHeight(SyncHeight);
                    }

                    // Loop through confirmed transactions
                    Dictionary<string, JObject> NewTransactions = new Dictionary<string, JObject>(ConfirmedTransactions);
                    foreach (KeyValuePair<string, JObject> ConfirmedTransaction in NewTransactions)
                    {
                        // Remove from list
                        ConfirmedTransactions.Remove(ConfirmedTransaction.Key);

                        // Check if transaction has not been added
                        if (!CheckTransactionExists(ConfirmedTransaction.Key))
                        {
                            // Get transaction
                            JObject Transaction = Request.RPC(walletHost, walletPort, "getTransaction",
                                new JObject { ["transactionHash"] = ConfirmedTransaction.Key }, walletRpcPassword);
                            if (!Transaction.HasValues || Transaction.Count < 1) continue;
                            else Transaction = (JObject)Transaction["transaction"];
                            Log(2, "Wallet", "Scanning transaction with tx {0}", ConfirmedTransaction.Key);

                            // Get transaction data
                            string PaymentId = (string)Transaction["paymentId"];
                            if (PaymentId == null) continue;//PaymentId = "";
                            decimal Fee = (decimal)Transaction["fee"] / coinUnits;

                            // Check if user exists in database
                            bool UserExists = false;
                            if (CheckUserExists(PaymentId)) UserExists = true;

                            // Get user balance
                            decimal Balance = GetBalance(PaymentId);
                            decimal OGBalance = Balance;

                            // Loop through transfers
                            foreach (JObject Transfer in Transaction["transfers"])
                            {
                                // Get transfer data
                                string Address = (string)Transfer["address"];
                                decimal Amount = (decimal)Transfer["amount"] / coinUnits;
                                decimal Change = 0;

                                // If address is a bot address (incoming)
                                if (tipAddresses.Contains(Address))
                                {
                                    Log(2, "Wallet", "Deposit of {0}", Amount);
                                    Change += Amount;
                                }

                                // Outgoing
                                else if (Address != "")
                                {
                                    Log(2, "Wallet", "Withdrawal of {0}", Amount);
                                    Change -= Fee;
                                }

                                // Set new balance
                                Balance += Change;
                            }

                            // Get transaction type
                            string Type = "OUT";
                            decimal Difference = Balance - OGBalance;
                            if (!CheckUserExists(PaymentId)) Type = "NO_PID";
                            else if (Difference > 0) Type = "IN";

                            // Update user if transaction is a deposit
                            if (UserExists && Type == "IN")
                            {
                                // Log deposit to console
                                Log(1, "Wallet", "Received a deposit of {0} for user {1} with pid {2}", Difference, GetUserId(PaymentId), PaymentId);

                                // Begin building a message
                                var Response = new EmbedBuilder();
                                Response.WithTitle("Deposit recieved!");
                                Response.Description = string.Format("Your deposit of **{0:N}** {1} has now been credited.\nNew balance: **{2:N}** {1}\nTX: **{3}**",
                                    Difference, coinSymbol, Balance, ConfirmedTransaction.Key);

                                // Send message
                                try { await _client.GetUser(GetUserId(PaymentId)).SendMessageAsync("", false, Response); }
                                catch { }

                                // Update global stats
                                try { GlobalStats("IN", 0, 0, GetUserId(PaymentId), Difference, 1); }
                                catch { }
                            }

                            // Update user balance
                            if (UserExists) SetBalance(PaymentId, Balance);

                            // Add transaction to database
                            LogTransaction((String)Transaction["timestamp"], Type, ConfirmedTransaction.Key, PaymentId, Difference);
                        }

                        // Pending tip has processed
                        else if (CheckIfPending(ConfirmedTransaction.Key))
                        {
                            // Get user list
                            List<string> PaymentIds = GetPendingPaymentIds(ConfirmedTransaction.Key);

                            // Loop through pending users
                            foreach (string PID in PaymentIds)
                            {
                                // Get user balance
                                decimal Balance = GetBalance(PID);

                                // Get pending amount
                                decimal Pending = GetPendingAmount(ConfirmedTransaction.Key);

                                // Set new balance
                                SetBalance(PID, Balance + Pending);

                                // Log deposit to console
                                Log(1, "Wallet", "Processed pending tip of {0} for user {1} with pid {2}", Pending, GetUserId(PID), PID);

                                // Begin building a message
                                var Response = new EmbedBuilder();
                                Response.WithTitle("Tip processed!");
                                Response.Description = string.Format("A tip of **{0:N}** {1} that was sent to you has now been credited.\nNew balance: **{2:N}** {1}\nTX: **{3}**",
                                    Pending, coinSymbol, Balance + Pending, ConfirmedTransaction.Key);

                                // Send message
                                try { await _client.GetUser(GetUserId(PID)).SendMessageAsync("", false, Response); }
                                catch { }

                                // Update global stats
                                try { GlobalStats("IN", 0, 0, GetUserId(PID), Pending, 1); }
                                catch { }
                            }

                            // Remove from pending tips
                            RemovePending(ConfirmedTransaction.Key);
                        }

                        // Transaction already in database
                        else
                        {
                            Log(2, "Wallet", "Skipping already existing tx {0}", ConfirmedTransaction.Key);
                            continue;
                        }
                    }

                    // Loop through unconfirmed transactions
                    NewTransactions = new Dictionary<string, JObject>(UnconfirmedTransactions);
                    foreach (KeyValuePair<string, JObject> UnconfirmedTransaction in NewTransactions)
                    {
                        // Get transaction info
                        JObject Transaction = Request.RPC(walletHost, walletPort, "getTransaction", new JObject { ["transactionHash"] = UnconfirmedTransaction.Key }, walletRpcPassword);

                        // Check confirmation status
                        if ((int)Transaction["unlockTime"] <= 3)
                        {
                            ConfirmedTransactions.Add((string)Transaction["transactionHash"], Transaction);
                            UnconfirmedTransactions.Remove(UnconfirmedTransaction.Key);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(2, "Wallet", "Error while performing update loop: {0}", e.Message);
                    continue;
                }

                // Wait for specified delay time
                await Task.Delay(walletUpdateDelay);
            }
        }
    }
}
