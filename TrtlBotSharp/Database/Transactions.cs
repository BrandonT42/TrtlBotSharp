using Microsoft.Data.Sqlite;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // Checks if a transaction exists in the database
        public static bool CheckTransactionExists(string TransactionHash)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT tx FROM transactions WHERE tx = @tx", Database);
            Command.Parameters.AddWithValue("tx", TransactionHash.ToUpper());

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.HasRows) return true;
            return false;
        }

        // Logs a transaction in the database
        public static void LogTransaction(string Timestamp, string Type, string TransactionHash, string PaymentId, decimal Amount)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("INSERT INTO transactions (createdat, type, tx, paymentid, amount) VALUES (@createdat, @type, @tx, @paymentid, @amount)", Database);
            Command.Parameters.AddWithValue("createdat", Timestamp);
            Command.Parameters.AddWithValue("type", Type);
            Command.Parameters.AddWithValue("tx", TransactionHash.ToUpper());
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());
            Command.Parameters.AddWithValue("amount", Amount);

            // Execute command
            Command.ExecuteNonQuery();
        }
    }
}
