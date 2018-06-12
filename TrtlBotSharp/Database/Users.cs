using Microsoft.Data.Sqlite;
using System.Linq;
using System.Collections.Generic;

namespace TrtlBotSharp
{
    public partial class TrtlBotSharp
    {
        // returns all uids for users with balance > 0
        public static List<ulong> GetActiveUsers(ulong UID)
        {
            List<long> results = new List<long>();
            SqliteCommand Command = new SqliteCommand("SELECT uid FROM users WHERE balance > 0 AND uid != @self", Database);
            Command.Parameters.AddWithValue("self", UID);
            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader()) 
            {
                int count = Reader.FieldCount;
                while(Reader.Read()) 
                {
                    for(int i = 0 ; i < count ; i++) 
                    {
                        results.Add((long)Reader.GetValue(i));
                    }
                }
            }
            
            return results.Select(i => (ulong)i).ToList();
        }
    }
}
