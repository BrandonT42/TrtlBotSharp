using Microsoft.Data.Sqlite;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // Gets last sync height
        public static int GetSyncHeight()
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT height FROM sync", Database);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetInt32(0);

            // Could not find height
            return 0;
        }

        // Sets last sync height
        public static void SetSyncHeight(int Height)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("UPDATE sync SET height = @height", Database);
            Command.Parameters.AddWithValue("height", Height);

            // Execute command
            Command.ExecuteNonQuery();
        }
    }
}
