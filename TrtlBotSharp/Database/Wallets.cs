using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // Checks if a user exists in the database
        public static bool CheckUserExists(ulong UID)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT uid FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.HasRows) return true;
            return false;
        }

        // Checks if a user exists in the database
        public static bool CheckUserExists(string PaymentId)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT uid FROM users WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.HasRows) return true;
            return false;
        }

        // Checks if an address exists in the database
        public static bool CheckAddressExists(string Address)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT address FROM users WHERE address = @address", Database);
            Command.Parameters.AddWithValue("address", Address);

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.HasRows) return true;
            return false;
        }

        // Registers a wallet into the database
        public static string RegisterWallet(ulong UID, string Address)
        {
            // Generate a new payment ID
            string PaymentId = GeneratePaymentId(Address);

            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("INSERT INTO users (uid, address, paymentid) VALUES (@uid, @address, @paymentid)", Database);
            Command.Parameters.AddWithValue("uid", UID);
            Command.Parameters.AddWithValue("address", Address);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());

            // Execute command
            Command.ExecuteNonQuery();

            // Return generated payment ID
            return PaymentId;
        }

        // Updates a wallet's address the database
        public static void UpdateWallet(ulong UID, string Address)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("UPDATE users SET address = @address WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);
            Command.Parameters.AddWithValue("address", Address);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Gets a wallet address from the database
        public static string GetAddress(ulong UID)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT address FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetString(0);

            // Could not find uid
            return "";
        }

        // Gets a user's balance from the database
        public static decimal GetBalance(ulong UID)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT balance FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetDecimal(0);

            // Could not find uid
            return 0;
        }

        // Gets a user's balance from the database
        public static decimal GetBalance(string PaymentId)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT balance FROM users WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetDecimal(0);

            // Could not find uid
            return 0;
        }

        // Sets a user's balance in the database
        public static void SetBalance(ulong UID, decimal Balance)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("UPDATE users SET balance = @balance WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);
            Command.Parameters.AddWithValue("balance", Balance);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Sets a user's balance in the database
        public static void SetBalance(string PaymentId, decimal Balance)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("UPDATE users SET balance = @balance WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());
            Command.Parameters.AddWithValue("balance", Balance);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Gets a user's payment id
        public static string GetPaymentId(ulong UID)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT paymentid FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetString(0);

            // Could not find uid
            return "";
        }

        // Gets a user's uid
        public static ulong GetUserId(string PaymentId)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT uid FROM users WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return (ulong)Reader.GetInt64(0);

            // Could not find payment id
            return 0;
        }

        // Gets a user's redirect preference from the database
        public static bool GetRedirect(ulong UID)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT redirect FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetBoolean(0);

            // Could not find uid
            return false;
        }

        // Sets a user's redirect preferemce in the database
        public static void SetRedirect(ulong UID, bool Redirect)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("UPDATE users SET redirect = @redirect WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);
            Command.Parameters.AddWithValue("redirect", Redirect);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Adds a tip to user's stats
        public static void TipStats(ulong Sender, ulong Recipient, decimal Amount)
        {
            // Update sender
            SQLiteCommand Command = new SQLiteCommand("SELECT tipssent, coinssent FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", Sender);
            using (SQLiteDataReader Reader = Command.ExecuteReader())
            {
                if (Reader.Read())
                {
                    // Get current stats
                    int TipsSent = Reader.GetInt32(0) + 1;
                    decimal CoinsSent = Reader.GetDecimal(1) + Amount;

                    // Update sender stats
                    Command = new SQLiteCommand("UPDATE users SET tipssent = @tipssent, coinssent = @coinssent WHERE uid = @uid", Database);
                    Command.Parameters.AddWithValue("uid", Sender);
                    Command.Parameters.AddWithValue("tipssent", TipsSent);
                    Command.Parameters.AddWithValue("coinssent", CoinsSent);
                    Command.ExecuteNonQuery();
                }
            }

            // Update recipient
            Command = new SQLiteCommand("SELECT tipsrecv, coinsrecv FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", Recipient);
            using (SQLiteDataReader Reader = Command.ExecuteReader())
            {
                if (Reader.Read())
                {
                    // Get current stats
                    int TipsRecv = Reader.GetInt32(0) + 1;
                    decimal CoinsRecv = Reader.GetDecimal(1) + Amount;

                    // Update sender stats
                    Command = new SQLiteCommand("UPDATE users SET tipsrecv = @tipsrecv, coinsrecv = @coinsrecv WHERE uid = @uid", Database);
                    Command.Parameters.AddWithValue("uid", Recipient);
                    Command.Parameters.AddWithValue("tipsrecv", TipsRecv);
                    Command.Parameters.AddWithValue("coinsrecv", CoinsRecv);
                    Command.ExecuteNonQuery();
                }
            }
        }
    }
}
