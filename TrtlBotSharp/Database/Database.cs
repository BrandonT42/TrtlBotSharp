using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // Database container
        public static SqliteConnection Database;

        // Loads the database
        public static Task LoadDatabase()
        {
            // Load database
            Database = new SqliteConnection("Data Source=" + databaseFile);// + ";Version=3;");
            Database.Open();

            // Attempt to create users table
            SqliteCommand UsersTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS users (uid INT, address TEXT, paymentid VARCHAR(64), " +
                "balance BIGINT DEFAULT 0, tipssent INT DEFAULT 0, tipsrecv INT DEFAULT 0, coinssent BIGINT DEFAULT 0, " +
                "coinsrecv BIGINT DEFAULT 0, redirect BOOLEAN DEFAULT 0)", Database);
            UsersTableCreationCommand.ExecuteNonQuery();

            // Attempt to create transactions table
            SqliteCommand TransactionsTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS transactions (createdat TIMESTAMP, type TINYTEXT, amount BIGINT, paymentid VARCHAR(64), " +
                "tx TEXT)", Database);
            TransactionsTableCreationCommand.ExecuteNonQuery();

            // Attempt to create tips (stats) table
            SqliteCommand TipsTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS tips (createdat TIMESTAMP, type TINYTEXT, serverid INT, channelid INT, userid INT, " +
                "amount BIGINT, recipients INT, totalamount BIGINT)", Database);
            TipsTableCreationCommand.ExecuteNonQuery();

            // Attempt to create pending tips table
            SqliteCommand PendingTipsTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS pendingtips (tx TEXT, paymentid VARCHAR(64), amount BIGINT DEFAULT 0)", Database);
            PendingTipsTableCreationCommand.ExecuteNonQuery();

            // Attempt to create sync table
            SqliteCommand SyncTableCreationCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS sync (height INT DEFAULT 1)", Database);
            SyncTableCreationCommand.ExecuteNonQuery();
            SqliteCommand SyncTableDefaultCommand = new SqliteCommand("INSERT INTO sync(height) SELECT 1 WHERE NOT EXISTS(SELECT 1 FROM sync WHERE height > 0)", Database);
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
