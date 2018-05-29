using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // Gets last sync height
        public static int GetSyncHeight()
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("SELECT height FROM sync", Database);

            // Execute command
            using (SQLiteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetInt32(0);

            // Could not find height
            return 0;
        }

        // Sets last sync height
        public static void SetSyncHeight(int Height)
        {
            // Create SQL command
            SQLiteCommand Command = new SQLiteCommand("UPDATE sync SET height = @height", Database);
            Command.Parameters.AddWithValue("height", Height);

            // Execute command
            Command.ExecuteNonQuery();
        }
    }
}
