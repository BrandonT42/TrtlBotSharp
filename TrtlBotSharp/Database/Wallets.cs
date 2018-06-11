using Microsoft.Data.Sqlite;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // Checks if a user exists in the database
        public static bool CheckUserExists(ulong UID)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT uid FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.HasRows) return true;
            return false;
        }

        // Checks if a user exists in the database
        public static bool CheckUserExists(string PaymentId)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT uid FROM users WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.HasRows) return true;
            return false;
        }

        // Checks if an address exists in the database
        public static bool CheckAddressExists(string Address)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT address FROM users WHERE address = @address", Database);
            Command.Parameters.AddWithValue("address", Address);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.HasRows) return true;
            return false;
        }

        // Registers a wallet into the database
        public static string RegisterWallet(ulong UID, string Address)
        {
            // Generate a new payment ID
            string PaymentId = GeneratePaymentId(Address);

            // Create Sql command
            SqliteCommand Command = new SqliteCommand("INSERT INTO users (uid, address, paymentid) VALUES (@uid, @address, @paymentid)", Database);
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
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("UPDATE users SET address = @address WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);
            Command.Parameters.AddWithValue("address", Address);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Gets a wallet address from the database
        public static string GetAddress(ulong UID)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT address FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetString(0);

            // Could not find uid
            return "";
        }

        // Gets a user's balance from the database
        public static decimal GetBalance(ulong UID)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT balance FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetDecimal(0);

            // Could not find uid
            return 0;
        }

        // Gets a user's balance from the database
        public static decimal GetBalance(string PaymentId)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT balance FROM users WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetDecimal(0);

            // Could not find uid
            return 0;
        }

        // Sets a user's balance in the database
        public static void SetBalance(ulong UID, decimal Balance)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("UPDATE users SET balance = @balance WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);
            Command.Parameters.AddWithValue("balance", Balance);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Sets a user's balance in the database
        public static void SetBalance(string PaymentId, decimal Balance)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("UPDATE users SET balance = @balance WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());
            Command.Parameters.AddWithValue("balance", Balance);

            // Execute command
            Command.ExecuteNonQuery();
        }

        // Gets a user's payment id
        public static string GetPaymentId(ulong UID)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT paymentid FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetString(0);

            // Could not find uid
            return "";
        }

        // Gets a user's uid
        public static ulong GetUserId(string PaymentId)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT uid FROM users WHERE paymentid = @paymentid", Database);
            Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return (ulong)Reader.GetInt64(0);

            // Could not find payment id
            return 0;
        }

        // Gets a user's redirect preference from the database
        public static bool GetRedirect(ulong UID)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT redirect FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetBoolean(0);

            // Could not find uid
            return false;
        }

        // Sets a user's redirect preferemce in the database
        public static void SetRedirect(ulong UID, bool Redirect)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("UPDATE users SET redirect = @redirect WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);
            Command.Parameters.AddWithValue("redirect", Redirect);

            // Execute command
            Command.ExecuteNonQuery();
        }
    }
}
