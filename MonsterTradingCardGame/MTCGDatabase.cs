using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonsterTradingCardGame
{

    public class MTCGDatabase
    {
        private static readonly string[] _sqlSetUpScript =
        {
            @"DROP TABLE packages, players, cards, cardcompendium, scoreboard, trades, decks;",
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
                id SERIAL PRIMARY KEY,
                cardAsJson1 VARCHAR(255) NOT NULL,
                cardAsJson2 VARCHAR(255) NOT NULL,
                cardAsJson3 VARCHAR(255) NOT NULL,
                cardAsJson4 VARCHAR(255) NOT NULL,
                cardAsJson5 VARCHAR(255) NOT NULL
            );
            ",
            @"
            CREATE TABLE IF NOT EXISTS cardcompendium 
            (
                cardID VARCHAR(36) NOT NULL,
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbIPAddress"></param>
        /// <param name="dbPortNumber"></param>
        /// <returns>
        /// true, if connection and setup were successful
        /// </returns>
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
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// true, if user exists 
        /// false, if user doesn't exist or internal error occured
        /// </returns>
        public bool CheckUsersExistance(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            lock (_DataBaseLock)
            {
                if (_dbConnection == null)
                {
                    Console.WriteLine("ERROR: Database Connection has not been cutoff");
                    return false;
                }

                Dictionary<string, string> queryParameterD = new Dictionary<string, string>
                {
                    { "username", username }
                };

                string sqlStatement = $"SELECT COUNT(*) FROM players WHERE username = @username;";

                if (int.TryParse(ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD), out int result))
                {
                    return 0 < result;
                }

                return false;
            }
        }
        public NpgsqlCommand FillSQLCommandWithParameters(string sqlCommand, Dictionary<string, string> queryParameterD)
        {
            NpgsqlCommand cmd = new NpgsqlCommand(sqlCommand, _dbConnection);
            if (sqlCommand.Contains('@'))
            {
                foreach (var parameter in queryParameterD)
                {
                    if (sqlCommand.Contains($"@{parameter.Key}"))
                    {
                        cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }
            }
            return cmd;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="queryParameterD"></param>
        /// <returns>string with singular result of query or error message</returns>
        public string ExecuteSqlCodeWithSingularResult(string sqlCommand, Dictionary<string, string> queryParameterD)
        {
            lock (_DataBaseLock)
            {
                string errmsg;
                if ((errmsg = doDatabaseSecurityCheck(sqlCommand)) != "")
                {
                    Console.WriteLine(errmsg);
                    return errmsg;
                }

                try
                {
                    _dbConnection.Open();
                    using (NpgsqlCommand cmd = FillSQLCommandWithParameters(sqlCommand, queryParameterD))
                    {
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            if (sqlCommand.Contains("COUNT(") ||
                                sqlCommand.Contains("SELECT amount "))
                            {
                                return "0";
                            }
                            return "";
                        }
                        return result.ToString();
                    }
                }
                catch (JsonSerializationException ex)
                {
                    Console.WriteLine($"ERROR occured during Json serialization:\n{ex.Message}");
                    return "Database-ERROR: \n" + ex.Message;
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine($"ERROR occured during query:\n{ex.Message}");
                    return "Database-ERROR: \n" + ex.Message;
                }
                catch (Exception ex)
                {
                    return "Database-ERROR: \n" + ex.Message;
                }

                finally
                {
                    _dbConnection.Close();
                }
            }
        }
        /// <summary>
        /// is used for retrieving database-queries with multiple row and or columns 
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="queryParameterD"></param>
        /// <returns>string with query result or error message</returns>
        public string ExecuteSQLCode(string sqlCommand, Dictionary<string, string> queryParameterD)
        {
            lock (_DataBaseLock)
            {
                string errmsg;
                if ((errmsg = doDatabaseSecurityCheck(sqlCommand)) != "")
                {
                    Console.WriteLine(errmsg);
                    return errmsg;
                }

                try
                {
                    _dbConnection.Open();
                    using (NpgsqlCommand cmd = FillSQLCommandWithParameters(sqlCommand, queryParameterD))
                    {
                        if (sqlCommand.Contains("SELECT"))
                        {
                            using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(cmd))
                            {
                                DataSet dataSet = new DataSet();
                                dataAdapter.Fill(dataSet);

                                string result = JsonConvert.SerializeObject(dataSet.Tables[0]);
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
                    Console.WriteLine($"ERROR occured during Json serialization:\n{ex.Message}");
                    return "Database-ERROR: \n" + ex.Message;
                }
                catch (Exception ex)
                {
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
                if (_dbConnection == null)
                {
                    Console.WriteLine("ERROR: Database Connection has not been cutoff");
                    return false;
                }

                string sqlStatement = $"SELECT COUNT(*) FROM {tableName};";
                Dictionary<string, string> _ = new Dictionary<string, string>();

                if (int.TryParse(ExecuteSqlCodeWithSingularResult(sqlStatement, _), out int result))
                {
                    return 0 == result;
                }
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="deck"></param>
        /// <returns></returns>
        public string GetDeck(string username, out List<Card> deck)
        {
            deck = new List<Card>();
            lock (_DataBaseLock)
            {
                if (_dbConnection == null)
                {
                    Console.WriteLine("ERROR: Database Connection has not been cutoff");
                    return "ERROR: We are facing database issues right now";
                }
                
                try
                {
                    string sqlCommand;
                    Dictionary<string, string> paramD = new Dictionary<string, string>
                    {
                        { "username", username }
                    };
                    Card card;
                    string currentJson;

                    for (int i = 1; i <= 4; i++)
                    {
                        sqlCommand = $"SELECT c.cardAsJson FROM cards c, decks d WHERE c.cardID = d.cardID{i} AND d.username = @username;";

                        currentJson = ExecuteSqlCodeWithSingularResult(sqlCommand, paramD);

                        if (currentJson.Contains("\"SpellType\": 0"))
                            card = JsonConvert.DeserializeObject<Spell>(currentJson);
                        else if (currentJson.Contains("\"SpellType\": 1"))
                            card = JsonConvert.DeserializeObject<BuffSpell>(currentJson);
                        else if (currentJson.Contains("\"SpellType\": 2"))
                            card = JsonConvert.DeserializeObject<TrapSpell>(currentJson);
                        else
                            card = JsonConvert.DeserializeObject<Monster>(currentJson);

                        deck.Add(card);
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    return "ERROR: \n" + ex.Message;
                }
            }
        }

        public bool RedistributeCards(List<Card> oldDeck, List<Card> newDeck, Dictionary<string, string> queryParameterD)
        {
            int oldCardAmountInDeck;
            int newCardAmountInDeck;
            string sqlStatement;

            if (newDeck.Count == 0)
            {
                // remove lost cards from cardcompendium
                foreach (Card card in oldDeck)
                {
                    if (queryParameterD.TryAdd("cardID", card.ID)) ;
                    else
                    {
                        queryParameterD["cardID"] = card.ID;
                    }

                    sqlStatement = $"SELECT amount FROM cardcompendium WHERE username = @Tokenname AND cardID = @cardID;";
                    if (int.TryParse(ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD), out int amountInTable))
                    {
                        if (amountInTable <= 1)
                            sqlStatement = $"DELETE FROM cardcompendium WHERE username = @Tokenname AND cardID = @cardID;";
                        else
                            sqlStatement = $"UPDATE cardcompendium SET amount = amount - 1 WHERE username = @Tokenname AND cardID = @cardID;";
                        if (ExecuteSQLCode(sqlStatement, queryParameterD).Contains("ERROR"))
                        {
                            return false;
                        }
                    }
                }

                sqlStatement = $"DELETE FROM decks WHERE username = @Tokenname;";
                if (ExecuteSQLCode(sqlStatement, queryParameterD).Contains("ERROR"))
                {
                    return false;
                }
                return true;
            }

            foreach (Card card in newDeck)
            {
                if (queryParameterD.TryAdd("cardID", card.ID)) ;
                else
                {
                    queryParameterD["cardID"] = card.ID;
                }

                oldCardAmountInDeck = oldDeck.Where(c => c.ID == card.ID).Count();
                newCardAmountInDeck = newDeck.Where(c => c.ID == card.ID).Count();

                if (oldCardAmountInDeck < newCardAmountInDeck) // new cards
                {
                    sqlStatement = $"SELECT COUNT(*) FROM cardcompendium WHERE username = @Tokenname AND cardID = @cardID;";

                    if (ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD) == "0")
                    {
                        sqlStatement = $"INSERT INTO cardcompendium (cardID, username, amount) VALUES (@cardID, @Tokenname, {newCardAmountInDeck});";
                    }

                    else
                    {
                        sqlStatement = $"UPDATE cardcompendium SET amount = amount + 1 WHERE username = @Tokenname AND cardID = @cardID;";
                    }

                    if (ExecuteSQLCode(sqlStatement, queryParameterD).Contains("ERROR"))
                    {
                        return false;
                    }
                    continue;
                }
                else if (newCardAmountInDeck < oldCardAmountInDeck) // less cards occures only in draw situations
                {
                    sqlStatement = $"SELECT amount FROM cardcompendium WHERE username = @Tokenname AND cardID = @cardID;";
                    if (int.TryParse(ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD), out int amountInTable))
                    {
                        if (amountInTable < newCardAmountInDeck)
                        {
                            sqlStatement = $"DELETE FROM cardcompendium WHERE username = @Tokenname AND cardID = @cardID;";
                        }
                        else
                        {
                            sqlStatement = $"UPDATE cardcompendium SET amount = amount - 1 WHERE username = @Tokenname AND cardID = @cardID;";
                        }
                        if (ExecuteSQLCode(sqlStatement, queryParameterD).Contains("ERROR"))
                        {
                            return false;
                        }
                    }
                    else
                        return false;

                    continue;
                }
            }

            // remove lost cards from decks table
            for (int i = 0; i < oldDeck.Count(); i++)
            {
                if (!newDeck.Contains(oldDeck[i]))
                {
                    sqlStatement = $"UPDATE decks SET cardID{i + 1} = '' WHERE username = @Tokenname;";
                    if (ExecuteSQLCode(sqlStatement, queryParameterD).Contains("ERROR"))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// checks if database != null and if sqlCommand contains DROP TABLE statement
        /// </summary>
        /// <returns> empty string "", if check ran successfully or error message</returns>
        private string doDatabaseSecurityCheck(string sqlCommand)
        {
            int index;
            if (_dbConnection == null)
            {
                return "ERROR: We are facing database issues right now";
            }

            else if ((index = sqlCommand.IndexOf("DROP TABLE ")) != -1)
            {
                return $"ERROR: sqlCommand: {sqlCommand} contains DROP TABLE at: {index}!";
            }

            return "";
        }
    }
}
