using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // Database container
        public static SQLiteConnection Database;

        // Loads the database
        public static Task LoadDatabase()
        {
            // Check if database file exists and create it if not
            if (!File.Exists(databaseFile))
                SQLiteConnection.CreateFile(databaseFile);

            // Load database
            Database = new SQLiteConnection("Data Source=" + databaseFile + ";Version=3;");
            Database.Open();

            // Attempt to create users table
            SQLiteCommand UsersTableCreationCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS users (uid INT, address TEXT, paymentid VARCHAR(64), " +
                "balance BIGINT DEFAULT 0, tipssent INT DEFAULT 0, tipsrecv INT DEFAULT 0, coinssent BIGINT DEFAULT 0, " +
                "coinsrecv BIGINT DEFAULT 0, redirect BOOLEAN DEFAULT 0)", Database);
            UsersTableCreationCommand.ExecuteNonQuery();

            // Attempt to create transactions table
            SQLiteCommand TransactionsTableCreationCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS transactions (createdat TIMESTAMP, type TINYTEXT, amount BIGINT, paymentid VARCHAR(64), " +
                "tx TEXT)", Database);
            TransactionsTableCreationCommand.ExecuteNonQuery();

            // Attempt to create pending tips table
            SQLiteCommand PendingTipsTableCreationCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS pendingtips (tx TEXT, paymentid VARCHAR(64), amount BIGINT DEFAULT 0)", Database);
            PendingTipsTableCreationCommand.ExecuteNonQuery();

            // Attempt to create sync table
            SQLiteCommand SyncTableCreationCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS sync (height INT DEFAULT 1)", Database);
            SyncTableCreationCommand.ExecuteNonQuery();
            SQLiteCommand SyncTableDefaultCommand = new SQLiteCommand("INSERT INTO sync(height) SELECT 1 WHERE NOT EXISTS(SELECT 1 FROM sync WHERE height > 0)", Database);
            SyncTableDefaultCommand.ExecuteNonQuery();

            // Completed
            return Task.CompletedTask;
        }

        // Closes the database connections
        public static void CloseDatabase()
        {
            // Close the database connection
            Database.Close();
        }
    }
}
