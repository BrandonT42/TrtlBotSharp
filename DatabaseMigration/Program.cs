using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;

namespace DatabaseMigration
{
    // Utility class for caching user data
    class User
    {
        public string Address { get; set; }
        public string PaymentId { get; set; }
        public decimal Balance { get; set; }
        public User(string Address)
        {
            this.Address = Address;
            PaymentId = "";
            Balance = 0;
        }
    }

    // Utility class for caching transaction data
    class Transaction
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public Transaction(string PaymentId, decimal Amount)
        {
            this.PaymentId = PaymentId;
            this.Amount = Amount;
        }
    }

    // Main database migration tool
    class Program
    {
        // Declare DB files
        static string OldDB, NewDB;

        // Decimal places
        static int decimalPlaces;

        // Cached data
        static Dictionary<ulong, User> Users;
        static Dictionary<string, Transaction> Transactions;

        // Floors to decimal place
        public static decimal Floor(decimal Input)
        {
            var r = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(Input * r) / r;
        }

        // Entry point
        static void Main(string[] args)
        {
            // Get arguments
            if (args.Length < 3) return;
            OldDB = args[0];
            NewDB = args[1];
            decimalPlaces = int.Parse(args[2]);

            // Create user cache
            Users = new Dictionary<ulong, User>();

            // Create transaction cache
            Transactions = new Dictionary<string, Transaction>();

            // Load original database
            using (SqliteConnection DB = new SqliteConnection("Data Source=" + OldDB + ";"))
            {
                // Open a connection
                DB.Open();

                // Get user addresses
                SqliteCommand WalletsTable = DB.CreateCommand();
                WalletsTable.CommandText = "SELECT userid, address FROM wallets";
                using (var ReaderOne = WalletsTable.ExecuteReader())
                {
                    // Loop through rows
                    while (ReaderOne.Read())
                    {
                        // Get user ID
                        ulong UserId = (ulong)ReaderOne.GetInt64(0);

                        // Get address
                        string Address = ReaderOne.GetString(1);

                        // Create user object in cache
                        Users.Add(UserId, new User(Address));

                        // Get tip jar information
                        SqliteCommand TipsTable = DB.CreateCommand();
                        TipsTable.CommandText = "SELECT paymentid, amount FROM tips WHERE userid = @userid";
                        TipsTable.Parameters.AddWithValue("userid", UserId);

                        // Execute command
                        using (var ReaderTwo = TipsTable.ExecuteReader())
                        {
                            // Get row data
                            if (ReaderTwo.Read())
                            {
                                // Get payment ID
                                Users[UserId].PaymentId = ReaderTwo.GetString(0).ToUpper();

                                // Get balance
                                try { Users[UserId].Balance = Floor(Convert.ToDecimal(ReaderTwo.GetInt64(1)) / (10 * decimalPlaces)); }
                                catch { Users[UserId].Balance = 0; }
                            }
                        }

                        // Update console
                        Console.WriteLine("Caching userid {0} with payment id {1}, address {2} and balance {3:N}",
                            UserId, Users[UserId].PaymentId, Users[UserId].Address, Users[UserId].Balance);
                    }
                }

                // Get transactions
                SqliteCommand TransactionsTable = DB.CreateCommand();
                WalletsTable.CommandText = "SELECT tx, paymentid, amount FROM transactions";
                using (var ReaderOne = WalletsTable.ExecuteReader())
                {
                    // Loop through rows
                    while (ReaderOne.Read())
                    {
                        // Get transaction data
                        string Tx = ReaderOne.GetString(0).ToUpper();
                        string PaymentId = "";
                        try { PaymentId = ReaderOne.GetString(1).ToUpper(); }
                        catch { }
                        decimal Amount = 0;
                        try { Amount = Floor(Convert.ToDecimal(ReaderOne.GetInt64(2)) / (10 * decimalPlaces)); }
                        catch { }

                        // Create transaction object in cache
                        Transactions.Add(Tx, new Transaction(PaymentId, Amount));

                        // Update console
                        Console.WriteLine("Caching tx {0} with payment id {1} and amount {2:N}",
                            Tx, PaymentId, Amount);
                    }
                }
            }

            // Load new database
            using (SqliteConnection DB = new SqliteConnection("Data Source=" + NewDB + ";"))
            {
                // Open a connections
                DB.Open();

                // Attempt to create users table
                SqliteCommand UsersTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS users (uid INT, address TEXT, " +
                    "paymentid VARCHAR(64), balance BIGINT DEFAULT 0, tipssent INT DEFAULT 0, tipsrecv INT DEFAULT 0, coinssent BIGINT " +
                    "DEFAULT 0, coinsrecv BIGINT DEFAULT 0, redirect BOOLEAN DEFAULT 0)", DB);
                UsersTableCreationCommand.ExecuteNonQuery();

                // Attempt to create transactions table
                SqliteCommand TransactionsTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS transactions " +
                    "(createdat TIMESTAMP, type TINYTEXT, amount BIGINT, paymentid VARCHAR(64), tx TEXT)", DB);
                TransactionsTableCreationCommand.ExecuteNonQuery();

                // Attempt to create tips (stats) table
                SqliteCommand TipsTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS tips (createdat TIMESTAMP, " +
                    "type TINYTEXT, serverid INT, channelid INT, userid INT, amount BIGINT, recipients INT, totalamount BIGINT)", DB);
                TipsTableCreationCommand.ExecuteNonQuery();

                // Attempt to create pending tips table
                SqliteCommand PendingTipsTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS pendingtips (tx TEXT, " +
                    "paymentid VARCHAR(64), amount BIGINT DEFAULT 0)", DB);
                PendingTipsTableCreationCommand.ExecuteNonQuery();

                // Attempt to create sync table
                SqliteCommand SyncTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS sync (height INT DEFAULT 1)", DB);
                SyncTableCreationCommand.ExecuteNonQuery();
                SqliteCommand SyncTableDefaultCommand = new SqliteCommand("INSERT INTO sync(height) SELECT 1 WHERE NOT EXISTS(SELECT 1 " +
                    "FROM sync WHERE height > 0)", DB);
                SyncTableDefaultCommand.ExecuteNonQuery();

                // Loop through cached user list
                foreach (KeyValuePair<ulong, User> User in Users)
                {
                    // Add user to users table
                    SqliteCommand Command = new SqliteCommand("INSERT INTO users (uid, address, paymentid, balance) VALUES (@uid, @address, " +
                        "@paymentid, @balance)", DB);
                    Command.Parameters.AddWithValue("uid", User.Key);
                    Command.Parameters.AddWithValue("address", User.Value.Address);
                    Command.Parameters.AddWithValue("paymentid", User.Value.PaymentId);
                    Command.Parameters.AddWithValue("balance", User.Value.Balance);
                    Command.ExecuteNonQuery();

                    // Update console
                    Console.WriteLine("Adding user to new db: userid {0} with payment id {1}, address {2} and balance {3:N}",
                        User.Key, User.Value.PaymentId, User.Value.Address, User.Value.Balance);
                }

                // Loop through cached transaction list
                foreach (KeyValuePair<string, Transaction> Transaction in Transactions)
                {
                    // Add user to users table
                    SqliteCommand Command = new SqliteCommand("INSERT INTO transactions (createdat, type, tx, paymentid, amount) VALUES " +
                        "(@createdat, @type, @tx, @paymentid, @amount)", DB);
                    Command.Parameters.AddWithValue("createdat", 0);
                    Command.Parameters.AddWithValue("type", "DB_MIGRATION");
                    Command.Parameters.AddWithValue("tx", Transaction.Key);
                    Command.Parameters.AddWithValue("paymentid", Transaction.Value.PaymentId);
                    Command.Parameters.AddWithValue("amount", Transaction.Value.Amount);
                    Command.ExecuteNonQuery();

                    // Update console
                    Console.WriteLine("Adding tx to new db: tx {0} with payment id {1}, amount {2:N}",
                        Transaction.Key, Transaction.Value.PaymentId, Transaction.Value.Amount);
                }
            }

            // Update console
            Console.WriteLine("Database migration complete\nPress any key to exit");

            // Wait for keypress to exit
            Console.ReadKey();
        }
    }
}
