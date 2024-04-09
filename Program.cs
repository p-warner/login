using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace Sanitization
{
    internal static class Program
    {
        static void Main()
        {
            SQLiteConnection conn = new SQLiteConnection("Data Source=:memory:;Version=3;");
            conn.Open();

            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE \"User\" (\"IdUser\" INTEGER NOT NULL, \"Username\" TEXT NOT NULL, \"Password\" TEXT NOT NULL, PRIMARY KEY(\"IdUser\" AUTOINCREMENT));";

            cmd.ExecuteNonQuery();

            InsertSampleData(ref cmd);

            int idUser = -1;
            string username;
            string password;

            while (true)
            {
                Console.WriteLine("Login:");

                Console.Write("\tUsername: ");
                username = Console.ReadLine() ?? "";

                Console.Write("\tPassword: ");
                password = Console.ReadLine() ?? "";

                username = Sanatize(username);
                password = Sanatize(password);

                cmd.CommandText = $"SELECT IdUser, Username, Password FROM User WHERE Username = '{username}' AND Password = '{password}';";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        idUser = Convert.ToInt32(reader[0]);
                        username = (string)reader[1];
                        password = (string)reader[2];
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\tLogin Failed\n");
                        Console.ResetColor();
                        continue;
                    }
                }

                break;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Logged in as: {username}\n");
            Console.ResetColor();

            cmd.CommandText = "SELECT IdUser, Username, Password FROM User;";
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    Console.WriteLine($"\t{reader[0]}\t{reader[1]}\t{reader[2]}");
            }

            conn.Close();

            Console.Write("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void InsertSampleData(ref SQLiteCommand cmd)
        {
            string[][] data = new string[5][];
            data[0] = new string[2] { "admin", "admin" };
            data[1] = new string[2] { "Bob", "BobPass" };
            data[2] = new string[2] { "Fred", "FredGuy" };
            data[3] = new string[2] { "Lorem", "Ipsum" };
            data[4] = new string[2] { "Bart", "ImBart" };

            foreach (string[] arry in data)
            {
                cmd.CommandText = $"INSERT INTO User (Username, Password) VALUES ('{arry[0]}', '{arry[1]}')";

                cmd.ExecuteNonQuery();
            }
        }

        private static string Sanatize(string input)
        {
            input = input.Replace("SELECT", "");
            input = input.Replace("DROP", "");
            input = input.Replace("DELETE", "");
            input = input.Replace("WHERE", "");

            return input;
        }

        private static bool IsValid(string input)
        {
            var pattern = @"^[a-zA-Z0-9_\.@-]*$";

            var reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return !reg.IsMatch(input.ToLower());
        }

    }
}
