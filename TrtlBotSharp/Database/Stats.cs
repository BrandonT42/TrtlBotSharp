using System;
using Microsoft.Data.Sqlite;

namespace TrtlBotSharp
{
    partial class TrtlBotSharp
    {
        // Update global tip stats
        public static void GlobalStats(string Type, ulong Server, ulong Channel, ulong Sender, decimal Amount, int Recipients)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("INSERT INTO tips (createdat, type, serverid, channelid, userid, amount, recipients, totalamount) " +
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
            SqliteCommand Command = new SqliteCommand("SELECT tipssent, coinssent FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", Sender);
            using (SqliteDataReader Reader = Command.ExecuteReader())
            {
                if (Reader.Read())
                {
                    // Get current stats
                    int TipsSent = Reader.GetInt32(0) + 1;
                    decimal CoinsSent = Reader.GetDecimal(1) + Amount;

                    // Update sender stats
                    Command = new SqliteCommand("UPDATE users SET tipssent = @tipssent, coinssent = @coinssent WHERE uid = @uid", Database);
                    Command.Parameters.AddWithValue("uid", Sender);
                    Command.Parameters.AddWithValue("tipssent", TipsSent);
                    Command.Parameters.AddWithValue("coinssent", CoinsSent);
                    Command.ExecuteNonQuery();
                }
            }

            // Update recipient
            Command = new SqliteCommand("SELECT tipsrecv, coinsrecv FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", Recipient);
            using (SqliteDataReader Reader = Command.ExecuteReader())
            {
                if (Reader.Read())
                {
                    // Get current stats
                    int TipsRecv = Reader.GetInt32(0) + 1;
                    decimal CoinsRecv = Reader.GetDecimal(1) + Amount;

                    // Update sender stats
                    Command = new SqliteCommand("UPDATE users SET tipsrecv = @tipsrecv, coinsrecv = @coinsrecv WHERE uid = @uid", Database);
                    Command.Parameters.AddWithValue("uid", Recipient);
                    Command.Parameters.AddWithValue("tipsrecv", TipsRecv);
                    Command.Parameters.AddWithValue("coinsrecv", CoinsRecv);
                    Command.ExecuteNonQuery();
                }
            }
        }
    }
}
