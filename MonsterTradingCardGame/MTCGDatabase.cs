using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonsterTradingCardGame
{
    
    class MTCGDatabase
    {
        private static readonly string[] _sqlSetUpScript = 
        {
            @"
            CREATE TABLE IF NOT EXISTS players 
            (
                username VARCHAR(50) PRIMARY KEY,
                password VARCHAR(50) NOT NULL,
                coinpurse INT NOT NULL,
                bio VARCHAR(250),
                image VARCHAR(10)
            );
            ",
            @"
            CREATE TABLE IF NOT EXISTS scoreboard 
            (
                username VARCHAR(50) PRIMARY KEY,
                amountofwins INT NOT NULL
            );
            ",
            @"
            CREATE TABLE IF NOT EXISTS trades
            (
                tradeID VARCHAR(36) PRIMARY KEY,
                cardID VARCHAR(36) NOT NULL,
                username VARCHAR(50) NOT NULL,
                mindamage INT NOT NULL
            );
            ",
            @"
            CREATE TABLE IF NOT EXISTS packages
            (
                id VARCHAR(36) PRIMARY KEY,
                cardAsJson1 VARCHAR(200) NOT NULL,
                cardAsJson2 VARCHAR(200) NOT NULL,
                cardAsJson3 VARCHAR(200) NOT NULL,
                cardAsJson4 VARCHAR(200) NOT NULL,
                cardAsJson5 VARCHAR(200) NOT NULL
            );
            ",
            @"
            CREATE TABLE IF NOT EXISTS cardcompendium 
            (
                cardID VARCHAR(36) PRIMARY KEY,
                username VARCHAR(50) NOT NULL,
                amount INT NOT NULL
            );
            ",
            @"
            CREATE TABLE IF NOT EXISTS decks
            (
                username VARCHAR(50) PRIMARY KEY,
                cardID1 VARCHAR(36),
                cardID2 VARCHAR(36),
                cardID3 VARCHAR(36),
                cardID4 VARCHAR(36)
            );
            ", 
            @"
            CREATE TABLE IF NOT EXISTS cards
            (
                cardID VARCHAR(36) PRIMARY KEY,
                cardAsJson VARCHAR(200) NOT NULL
            );
            "
        };

        private static object _DataBaseLock = new object();
        private NpgsqlConnection? _dbConnection;

        public bool EstablishConnection(string dbIPAddress, int dbPortNumber)
        {
            Console.WriteLine("Enter the credentials of a DB-User with CONNECT permission:");
            Console.WriteLine("Username:");
            string username = Console.ReadLine();
            Console.WriteLine("Password:");
            string password = Console.ReadLine();

            string connectionString = $"Host={dbIPAddress}:{dbPortNumber};Username={username};Password={password};Database=mtcgdb;Include Error Detail=True";

            lock (_DataBaseLock)
            {
                _dbConnection = new NpgsqlConnection(connectionString);

                try
                {
                    _dbConnection.Open();
                    
                    foreach (string sqlTable in _sqlSetUpScript)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(sqlTable, _dbConnection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Data setup completed!");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: could not connect to Database");
                    return false;
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
        }
        public bool checkUsersExistance(string username)
        {
            if(string.IsNullOrEmpty(username))
                return false;
            lock (_DataBaseLock)
            {
                Dictionary<string, string> parameterD = new Dictionary<string, string> 
                {
                    { "username", username }
                };
                string sqlStatement = $"SELECT COUNT(*) FROM players WHERE username = @username;";
                string result = ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                
                return 0 < parseCountResult(result);
            }
        }
        public string ExecuteSQLCodeSanitized(string sqlCommand, Dictionary<string, string> parameterD)
        {
            lock (_DataBaseLock)
            {
                if (_dbConnection == null)
                {
                    Console.WriteLine("ERROR: Database Connection has not been established successfully");
                    return "We are facing database issues right now";
                }
                try
                {
                    _dbConnection.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCommand, _dbConnection))
                    {
                        Console.WriteLine("\n Commmand: " + sqlCommand + "\n");
                        if (sqlCommand.Contains('@'))
                        {
                            foreach (var parameter in parameterD)
                            {
                                if(sqlCommand.Contains($"@{parameter.Key}"))
                                {
                                    cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                    Console.WriteLine($"value: {parameter.Value}, was put into field: @{parameter.Key};");
                                }
                            }
                        }

                        if (sqlCommand.Contains("SELECT"))
                        {
                            using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(cmd))
                            {
                                DataSet dataSet = new DataSet();
                                dataAdapter.Fill(dataSet);
                                
                                string result = JsonConvert.SerializeObject(dataSet, Formatting.Indented);
                                Console.WriteLine(result);

                                if (sqlCommand.Contains("count(", System.StringComparison.CurrentCultureIgnoreCase))
                                    return ""+ parseCountResult(result);

                                return result;
                            }
                        }
                        else
                        {
                            return "" + cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (JsonSerializationException ex)
                {
                    Console.WriteLine($"ERROR: occured during Json serialization:\n{ex.Message}");
                    return "Database-ERROR: \n" + ex.Message;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: occured during query:\n{ex.Message}");
                    return "Database-ERROR: \n" + ex.Message;
                }

                finally
                {
                    _dbConnection.Close();
                }
            }
        }
        /// <summary>
        /// checks given Table for emptiness
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>true, if Table with specified tableName is empty and false if it is not empty or something unexpected happened</returns>
        public bool IsTableEmpty(string tableName)
        {
            lock (_DataBaseLock)
            {
                string sqlStatement = $"SELECT COUNT(*) FROM {tableName};";
                string result = ExecuteSQLCodeSanitized(sqlStatement, new Dictionary<string, string>());

                return parseCountResult(result) == 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="countResult"></param>
        /// <returns>parsed Count OR -1 if error occured</returns>
        private int parseCountResult(string countResult)
        {
            if (string.IsNullOrEmpty(countResult)                ||
                                     countResult.Contains("ERROR"))
                return -1;
            try
            {
                JObject jsonData = (JObject)JsonConvert.DeserializeObject(countResult);

                if (!int.TryParse(jsonData["Table"][0]["count"].ToString(), out int rowCount))
                    return -1;

                return rowCount;
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"Error deserializing JSON during parseCountResult(): {ex.Message}");
                return -1;
            }
        }

        public string GetDeckForBattle(string username, out List<Card> deck)
        {
            deck = new List<Card>();
            lock (_DataBaseLock)
            {
                if (_dbConnection == null)
                {
                    Console.WriteLine("ERROR: Database Connection has not been established successfully");
                    return "ERROR: Database doesn't work currently";
                }
                try
                {
                    _dbConnection.Open();

                    string sqlCommand;
                    for (int i = 1; i < 5; i++)
                    {
                        sqlCommand = $"SELECT c.cardAsJson FROM card c, deck d WHERE c.cardID = d.cardID{i} AND d.username = {username};";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCommand, _dbConnection))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                Card card;
                                string json = reader.GetString(0);
                                if (string.IsNullOrEmpty(json))
                                    return "ERROR: something went wrong while retrieving the deck from the database";

                                if ((card = (Card)JsonConvert.DeserializeObject(json)) == null)
                                    return "ERROR: something went wrong while retrieving the deck from the database";
                                deck.Add(card);
                            }
                        }
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: occured while retrieving data:\n{ex.Message}");
                    return "ERROR: \n" + ex.Message;
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
        }
    }
}
