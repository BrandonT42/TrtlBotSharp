using System;
using System.Data.SQLite;

namespace TrtlBotSharp
{
    partial class TrtlBotSharp
    {
        // Update global tip stats
        public static void GlobalStats(string Type, ulong Server, ulong Channel, ulong Sender, decimal Amount, int Recipients)
        {
            // Create Sql command
            SQLiteCommand Command = new SQLiteCommand("INSERT INTO tips (createdat, type, serverid, channelid, userid, amount, recipients, totalamount) " +
                "VALUES (@createdat, @type, @serverid, @channelid, @userid, @amount, @recipients, @totalamount)", Database);
            Command.Parameters.AddWithValue("createdat", (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            Command.Parameters.AddWithValue("type", Type);
            Command.Parameters.AddWithValue("serverid", Server);
            Command.Parameters.AddWithValue("channelid", Channel);
            Command.Parameters.AddWithValue("userid", Sender);
            Command.Parameters.AddWithValue("amount", Amount);
            Command.Parameters.AddWithValue("recipients", Recipients);
            Command.Parameters.AddWithValue("totalamount", Amount * Recipients);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Update user stats
        // Adds a tip to user's stats
        public static void UserStats(ulong Sender, ulong Recipient, decimal Amount)
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
