using System;
using System.Text;
using System.Net;
using System.Text.Json;

namespace MonsterTradingCardGame
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Drawing;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Numerics;
    using System.Reflection.Metadata;
    using System.Security.AccessControl;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.Json.Nodes;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Npgsql;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

    class MTCGServer
    {
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private static List<Socket> _lobbyClients = new List<Socket>();
        
        private static object _DataBaseLock = new object();
        private NpgsqlConnection? _dbConnection;
        
        private static object _TokenLock = new object();
        private static Dictionary<string, DateTime> _userTokensD = new Dictionary<string, DateTime>();

        private object _PoolLock = new object();
        private Queue<User> _waitingPlayers = new Queue<User>();
        
        private readonly int _TOKENACTIVITYTIMER = 300000;
        public async Task StartServer(string ipAddress, int portNumber, string dbIPAddress, int dbPortNumber)
        {
            if (IPAddress.TryParse(ipAddress, out IPAddress serverIP)  &&
                          ValidatePORT(portNumber)                     &&
                          ValidateIP(dbIPAddress)                      &&
                          ValidatePORT(dbPortNumber)                   &&
                          EstablishConnection(dbIPAddress, dbPortNumber))
            {
                Socket serverSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );
                serverSocket.Bind(
                    new IPEndPoint(
                        serverIP,
                        portNumber
                    )
                );
                serverSocket.Listen(10);

                Console.WriteLine("Server started...");

                while (true)
                {
                    Socket clientSocket = await serverSocket.AcceptAsync();
                    Console.WriteLine("Client connected.");

                    _ = Task.Run(() => HandleClientAsync(clientSocket));
                }
            }
            else 
            {
                Console.WriteLine("ERROR occured while starting the server");
                if (!ValidateIP(dbIPAddress))
                    Console.WriteLine($"-> the Database ip-address: {dbIPAddress} is invalid!");
                if (!ValidatePORT(dbPortNumber))
                    Console.WriteLine($"-> the Database port: {dbPortNumber} is invalid!");
                if (!ValidateIP(ipAddress))
                    Console.WriteLine($"-> the Server ip-address: {ipAddress} is invalid!");
                if (!ValidatePORT(portNumber))
                    Console.WriteLine($"-> the Server port: {portNumber} is invalid!");
            }
        }
        // DATABASE CONNECTION FUNCTIONS:
        private bool EstablishConnection(string dbIPAddress, int dbPortNumber)
        {
            Console.WriteLine("Enter the following credentials of a DB-User with CONNECT permission:");
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

                    using (NpgsqlCommand command = new NpgsqlCommand(Database._sqlScript, _dbConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: could not connect to Database");
                }
                finally
                {
                    _dbConnection.Close();
                }
                return false;
            }
        }
        private string ExecuteSQLCodeSanitized(string sqlCommand, Dictionary<string, string> parameterD)
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
                        foreach (var parameter in parameterD)
                        {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                            Console.WriteLine(parameter.Key + " : " + parameter.Value);
                        }
                        if (sqlCommand.Contains("SELECT"))
                        {
                            using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(cmd))
                            {
                                DataSet dataSet = new DataSet();
                                dataAdapter.Fill(dataSet);

                                return JsonConvert.SerializeObject(dataSet, Formatting.Indented);
                            }
                        }
                        else
                        {
                            string response = "" + cmd.ExecuteNonQuery();
                            //Console.WriteLine("\nDataBaseResponse: " + response + "\n");// to get the funky response
                            return response;
                        }
                    }
                }
                catch (JsonSerializationException ex)
                {
                    return "JSON couldn't be serialized";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: occured while inserting data:\n{ex.Message}");
                    return "ERROR: \n" + ex.Message;
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
        /// <returns>true, if Table is Empty and false if not or something unexpected happened</returns>
        public bool IsTableEmpty(string tableName)
        {
            lock (_DataBaseLock)
            {
                string sqlStatement = $"SELECT COUNT(*) FROM {tableName};";

                Dictionary<string, string> parameterD = new Dictionary<string, string>();

                string result = ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                if (int.TryParse(result, out int rowCount))
                    return rowCount == 0;
                return false;
            }
        }
        private string GetDeckForBattle(string username, out List<Card> deck)
        {
            deck = new List<Card>();
            lock (_DataBaseLock)
            {
                if (_dbConnection == null)
                {
                    Console.WriteLine("ERROR: Database Connection has not been established successfully");
                    return "ERROR: Connection is Empty";
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
                    Console.WriteLine($"ERROR: occured while inserting data:\n{ex.Message}");
                    return "ERROR: \n" + ex.Message;
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
        }


        // TOKEN HANDLING FUNCTIONS:

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        private static void SetToken(string userToken)
        {
            lock (_TokenLock)
            {
                if (_userTokensD.ContainsKey(userToken))
                    _userTokensD[userToken] = DateTime.Now;
                else
                    _userTokensD.Add(userToken, DateTime.Now);
            }
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        private bool IsTokenActive(string userToken)
        {
            lock (_TokenLock)
            {
                return (_userTokensD.ContainsKey(userToken)                                             &&
                       (_userTokensD[userToken] - DateTime.Now).TotalMilliseconds > _TOKENACTIVITYTIMER );
            }

        }

        // HTTP COMMUNICATION FUNCTION:
        private async Task HandleClientAsync(Socket clientSocket)
        {
            try
            {
                await semaphoreSlim.WaitAsync();

                byte[] buffer = new byte[1024];
                int length = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                string request = Encoding.UTF8.GetString(buffer, 0, length);

                string data = ProcessRequest(request, out int number);
                if (number == null          ||
                    data.Contains("ERROR"   ))
                    number = -1; // ERROR
                string response = $"HTTP/1.1 {number} \r\nContent-Type: text/plain -d " + data;

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await clientSocket.SendAsync(new ArraySegment<byte>(responseBytes), SocketFlags.None);

                Console.WriteLine($"Response: {response} sent.");

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                Console.WriteLine("Client disconnected.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error handling client: {e.Message}");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        // HTTP-REQUEST PROCESSING FUNCTIONS:
        private string ProcessRequest(string request, out int number)
        {
            number = 0;
            string errmsg = "";

            if (string.IsNullOrEmpty(request))
                return "ERROR: request is null or empty";

            if ((errmsg = ExtractHTTPdata(request, out Dictionary<string, string> httpDataD)) != "")
                return errmsg;

            if ((errmsg = TurnHttpDataDtoParameterD(httpDataD, out Dictionary<string, string> parameterD)) != "")
               return errmsg;

            switch (httpDataD["HTTP-method"])
            {
                case "POST":
                    return HandlePOSTRequest(httpDataD["path"], parameterD, ref number);
                case "GET":
                    return HandleGETRequest(httpDataD["path"], parameterD, ref number);
                case "PUT":
                    return HandlePUTRequest(httpDataD["path"], parameterD, ref number);
                case "DELETE":
                    return HandleDELETERequest(httpDataD["path"], parameterD, ref number);
                default:
                    return "ERROR: HTTPMETHOD is not in the api spec";
            }
        }

        private string HandlePOSTRequest(string path, Dictionary<string, string> parameterD, ref int number)
        {
            string sqlStatement;
            switch (path)
            {
                case "/users":
                    sqlStatement = "INSERT INTO player (username, password, coinpurse) VALUES (@username, @password, 20);";
                    if (ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR:"))
                    {
                        number = 409;
                        return "USER ALREADY EXISTS";
                    }
                    number = 200;
                    return "SUCCESS";
                case "/tradings":
                    if (!parameterD.ContainsKey("Tokenname") &&
                       !IsTokenActive(parameterD["Tokenname"]))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }
                    sqlStatement = "INSERT INTO trade (tradeID, cardID, username, mindamage) VALUES (@tradeID, @cardID, @Tokenname, @minDamage);";
                    return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/battles":
                    if(!parameterD.ContainsKey("Tokenname")  &&
                       !IsTokenActive(parameterD["Tokenname"]))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    GetDeckForBattle(parameterD["Tokenname"], out List<Card> deck);

                    User player = new User(parameterD["Tokenname"], deck);
                    AddPlayer(player);
                    Battle currentBattle = WaitForOpponent();
                    string winner = currentBattle.StartBattle();
                    // TODO: implement Cardloss
                    sqlStatement = $"UPDATE scoreboard SET amountofwins = amountofwins + 1 WHERE username = {winner};";

                    return currentBattle.BattleLog;

                case "/transactions/packages":

                    if (!parameterD.ContainsKey("Tokenname")  &&
                        !IsTokenActive(parameterD["Tokenname"]))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    sqlStatement = $"SELECT coinpurse FROM player WHERE username = @Tokenname";
                    if (!(int.TryParse(ExecuteSQLCodeSanitized(sqlStatement, parameterD), out int coins) &&
                        coins > 5                                                                        ))
                    {
                        number = 403;
                        return "NOT ENOUGH MONEY";
                    }
                    if (IsTableEmpty("package"))
                    {
                        number = 404;
                        return "TABLE IS EMPTY";
                    }

                    sqlStatement = $"UPDATE player SET coinpurse = coinpurse - 5  WHERE username = @Tokenname;";
                    number = 404;

                    return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/packages":
                    Console.WriteLine("\n/packages\n");
                    if (!parameterD.ContainsKey("Tokenname")  &&
                        !IsTokenActive(parameterD["Tokenname"]))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    if (!(parameterD["Tokenname"] == "admin"))
                    {
                        number = 403;
                        return "You don't have the privileges for that";
                    }


                    sqlStatement = "INSERT INTO package (id, cardAsJson1, cardAsJson2, cardAsJson3, cardAsJson4, cardAsJson5) VALUES (@id, @cardasjson1, @cardasjson2, @cardasjson3, @cardasjson4, @cardasjson5);";

                    string errmsg;
                    if((errmsg = ExecuteSQLCodeSanitized(sqlStatement, parameterD)).Contains("ERROR"))
                        return errmsg;

                    sqlStatement = "INSERT INTO cards (cardID, cardAsJson) VALUES (@cardid, @cardasjson)";
                    bool cardAlreadyExisted = false;
                    var currentCardD = new Dictionary<string, string>();
                    string key = "id";
                    foreach (var item in parameterD)
                    {
                        if (item.Key != "Tokenname" ||
                            item.Key != key         )
                        {                 
                            currentCardD.Add(key, ExtractValueOutOfJsonString(item.Value, "id"));
                            currentCardD.Add("cardasjson", item.Value);

                            if (ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR"))
                                cardAlreadyExisted = true;
                            currentCardD = new Dictionary<string, string>();
                        }
                    }

                    if (cardAlreadyExisted)
                    {
                        number = 409;
                        return "ONE OR MORE CARDS ALREADY EXISTED";
                    }
                    else
                    {
                        number = 409;
                        return "SUCCESS";
                    }


                case "/sessions":
                    sqlStatement = "SELECT COUNT(*) FROM player WHERE username = @username AND password = @password;";

                    if (int.TryParse(ExecuteSQLCodeSanitized(sqlStatement, parameterD), out int result) &&
                        result > 0                                                                       )
                    {
                        SetToken(parameterD["username"]);
                        number = 200;
                        return "SUCCESS";
                    }
                    else
                    {
                        number = 401;
                        return "INVALID USERNAME / PASSWORD";
                    }

                default:
                    if(path.Contains("/tradings/"))
                    {
                        if (!parameterD.ContainsKey("Tokenname")  &&
                            !IsTokenActive(parameterD["Tokenname"]))
                        {
                            number = 401;
                            return "INVALID TOKEN";
                        }
                        string tradeID = GetDynamicDataFromPath(path);
                        sqlStatement = "";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                    }
                    else return "Unknown path For POST HTTP-method";
            }
        }
        
        private string HandleGETRequest(string path, Dictionary<string, string> parameterD, ref int number)
        {
            
            if (parameterD.ContainsKey("Tokenname")  &&
                IsTokenActive(parameterD["Tokenname"]))
            {
                string sqlStatement;
                switch (path)
                {
                    case "/tradings":
                        sqlStatement = "SELECT * FROM trade WHERE username != @Tokenname;";
                        string query;
                        if ((query = ExecuteSQLCodeSanitized(sqlStatement, parameterD)).Contains("ERROR"))
                            return query;
                        if (query == "JSON couldn't be serialized") 
                            number = 204;
                        return query;

                    case "/scoreboard":
                        sqlStatement = "SELECT * FROM scoreboard ORDER BY amountofwins DESC;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/stats":
                        sqlStatement = "SELECT * FROM scoreboard WHERE username = @Tokenname;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/deck?format=plain":
                        sqlStatement = "SELECT * FROM scoreboard WHERE username = @Tokenname;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/deck":
                        sqlStatement = "SELECT cardID1, cardID2, cardID3, cardID4 FROM deck WHERE username = @Tokenname;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/cards":
                        sqlStatement = "SELECT c.cardAsJson FROM cardcompendium cc, cards c WHERE  c.cardID = cc.cardID AND username = @Tokenname;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    default:
                        if (path.Contains("/users/"))
                        {
                            sqlStatement = "SELECT * FROM player WHERE username = @Tokenname;";
                            return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                            
                        }
                        else
                            return "Unknown path For GET HTTP-method";
                }
            }
            else
            {
                number = 401;
                return "INVALID TOKEN";
            }
        }
        
        private string HandlePUTRequest(string path, Dictionary<string, string> parameterD, ref int number)
        {
            string sqlStatement;
            if (path == "/deck")
            {
                if (!parameterD.ContainsKey("Tokenname")  &&
                    !IsTokenActive(parameterD["Tokenname"]))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }

                int i = 1;
                foreach (var item in parameterD)
                {
                    if (item.Key != "Tokenname")
                    {
                        sqlStatement = $"SELECT amount FROM deck WHERE cardID = @cardID{i} AND username = @Tokenname;";

                        if (ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR"))
                        {
                            number = 403;
                            return "";
                        }
                        ++i;
                    }
                }

                sqlStatement = "SELECT Count(*) FROM deck WHERE username = @Tokenname;"; 
                if (ExecuteSQLCodeSanitized(sqlStatement, parameterD) == "0")
                    sqlStatement = "INSERT INTO deck (username, cardID1, cardID2, cardID3, cardID4) VALUES (@Tokenname, @cardid1, @cardid2, @cardid3, @cardid4);";
                else
                    sqlStatement = "UPDATE deck SET cardID1 = @cardid1, cardID2 = @cardid2, cardID3 = @cardid3, cardID4 = @cardid4 WHERE username = @Tokenname;";
                if (ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR"))
                {
                    number = 400;
                    return "";
                }
                number = 200;
                return "SUCCESS";

            }
            else if (path.Contains("/users/"))
            {
                if (!parameterD.ContainsKey("oldusername")  &&
                    !IsTokenActive(parameterD["oldusername"]))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }
                sqlStatement = $"UPDATE player SET username = @username, password = @password, bio = @bio, image = @image WHERE username = @oldusername;";
                return ExecuteSQLCodeSanitized(sqlStatement, parameterD);    
            }
            else return "Unknown path for PUT HTTP-method";
        }
        private string HandleDELETERequest(string path, Dictionary<string, string> parameterD, ref int number)
        {
            if (path.Contains("/tradings/"))
            {
                if (!parameterD.ContainsKey("Tokenname")  &&
                    !IsTokenActive(parameterD["Tokenname"]))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }
                string id = GetDynamicDataFromPath(path);
                string sqlStatement = $"DELETE FROM trade WHERE tradeID = {id};";
                return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
            }   
            else return "Unknown path for DELETE HTTP-method";
        }
        // JSON HANDLER FUNCTIONS:

        public static string TurnHttpDataDtoParameterD(Dictionary<string, string> httpDataD, out Dictionary<string, string> parameterD)
        {
            parameterD = new Dictionary<string, string>();

            if (!httpDataD.TryGetValue("JSON-Data", out string jsonAsString))
                return "ERROR: No Key named JSON-Data was found in httpDataD";

            try
            {
                if (jsonAsString.Contains("["))
                {
                    JArray jsonData = (JArray) JsonConvert.DeserializeObject(jsonAsString);
                    
                    if ((parameterD = GetParametersFromJsonArray(jsonData, httpDataD["path"])) == null)
                        return "ERROR: the parameters of the provided JSON couldn't be parsed";
                }
                else
                {
                    JObject jsonData = (JObject) JsonConvert.DeserializeObject(jsonAsString);

                    if ((parameterD = GetParametersFromJsonObject(jsonData)) == null)
                        return "ERROR: the parameters of the provided JSON couldn't be parsed";
                }
                if (httpDataD.ContainsKey("Tokenname"))
                    parameterD.Add("Tokenname", httpDataD["Tokenname"]);
                return "";
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return "ERROR: the provided JSON couldn't be deserialized, please check the Syntax";
            }
        }

        public static Dictionary<string, string> GetParametersFromJsonObject(JObject jsonData)
        {
            var parameterD = new Dictionary<string, string>();
            foreach (var property in jsonData.Properties())
            {
                parameterD.Add(property.Name.ToLower(), property.Value.ToString());
            }

            return parameterD;
        }
        public static Dictionary<string, string> GetParametersFromJsonArray(JArray jsonArray, string path)
        {
            var outputD = new Dictionary<string, string>();
            if (path == "/packages")
            {
                int index = 1;
                outputD.Add("id", jsonArray.First().ToString());
                jsonArray.RemoveAt(index);
                foreach (JObject element in jsonArray)
                {
                    outputD.Add("cardasjson" + index++, element.ToString());
                }
            }
            else
            {
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    outputD.Add("cardID"+(i+1), jsonArray[i].ToString());
                }
            }

            return outputD;
        }
        // POOL HELPER FUNCTIONS:
        public Battle WaitForOpponent()
        {
            lock (_PoolLock)
            {
                while (_waitingPlayers.Count < 2)
                    Monitor.Wait(_PoolLock);

                List<User> players = _waitingPlayers.ToList();
                _waitingPlayers.Clear();
                Battle battle = new Battle(players[1], players[2]) { };
                return battle;
            }
        }
        public void AddPlayer(User player)
        {
            lock (_PoolLock)
            {
                _waitingPlayers.Enqueue(player);
                Monitor.PulseAll(_PoolLock);
            }
        }

        // STRING PARSING FUNCTIONS:
        public static string GetDynamicDataFromPath(string path)
        {
            return path[(path.LastIndexOf('/') + 1)..(path.Length - 1)];
        }
        public static string ExtractValueOutOfJsonString(string jsonString, string key)
        {
            if (!jsonString.Contains(key))
                return "";

            int start = jsonString.IndexOf(key) + 5;
            int lengthOfCardID = 0;

            for (int i = start; i < jsonString[i]; i++)
            {
                if (jsonString[i] == '"')
                {
                    lengthOfCardID = i-1;
                    break;
                }
            }
            return jsonString[start..lengthOfCardID];
        }

        public static string ExtractHTTPdata(string request, out Dictionary<string, string> httpDataD)
        {
            httpDataD = new Dictionary<string, string>();
            string httpMethod = "", path = "";

            int indexHelper = 0;

            for (int i = 0; i < request.Length && indexHelper < 3; i++)
            {
                if (request[i] == ' ')
                    ++indexHelper;
                else if (indexHelper == 0)
                    httpMethod += request[i];
                else if (indexHelper == 1)
                    path += request[i];
            }
            httpDataD.Add("HTTP-method", httpMethod);
            httpDataD.Add("path", path);

            if (request.Contains("Authorization: Bearer "))
            {
                
                string username = "";
                string restOftheToken = "";
                bool minusHasBeenReached = false;

                if ((indexHelper = request.IndexOf("Authorization: Bearer ")) == -1)
                    return "FORMATTING-ERROR: Authorization Field was found, but has no index...";

                for (int i = indexHelper + 22;
                         i < request.Length &&
                         request[i] != (char)13; i++)
                {
                    if (request[i] == '-')
                        minusHasBeenReached = true;
                    if(minusHasBeenReached)
                        restOftheToken += request[i];
                    else
                        username += request[i];
                }
                if(restOftheToken == "-mtcgToken")
                    httpDataD.Add("Tokenname", username);
            }

            if (request.Contains("Content-Type: application/json"))
            {

                string lengthAsString = "";

                // Underneath indexHelper is used to capture the index of "Content-Length: "
                if ((indexHelper = request.IndexOf("Content-Length: ")) == -1)
                    return "FORMATTING-ERROR: Content-Length Field was found, but has no index...";
                
                for (int i = indexHelper + 16;
                         i < request.Length &&
                         request[i] != (char)13; i++)
                    // Underneath indexHelper is used to save the length of Json data
                    lengthAsString += request[i];

                // Underneath indexHelper is used to save the length of Json data
                if (!int.TryParse(lengthAsString, out indexHelper))
                    return "PARSING-ERROR: Could not Parse Content-Length";

                httpDataD.Add("JSON-Data", $"{request[(1 + request.Length - (indexHelper))..(request.Length - 1)].Replace("\\", string.Empty)}");
            }

            return "";
        }
        public bool ValidatePORT(int input)
        {
            return (0 <= input && input <= 65535);
        }
        public bool ValidateIP(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            IPAddress _;
            return IPAddress.TryParse(input, out _);
        }
    }
}
