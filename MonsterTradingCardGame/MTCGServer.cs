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

        private static MTCGDatabase _DatabaseHandler = new MTCGDatabase(); 
        
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
                          _DatabaseHandler.EstablishConnection(dbIPAddress, dbPortNumber))
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
                    Console.WriteLine("Client not yet connected.");
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

        private bool IsTokenActive(string userToken)
        {
            lock (_TokenLock)
            {
                return (_userTokensD.ContainsKey(userToken)                                             &&
                       (_userTokensD[userToken] - DateTime.Now).TotalMilliseconds > _TOKENACTIVITYTIMER );
            }
        }

        private bool doTokenCheckIn(Dictionary<string, string> parameterD) 
        {
            return !parameterD.ContainsKey("Tokenname") && !IsTokenActive(parameterD["Tokenname"]);
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
            number = -1;
            string errmsg = "";

            if (string.IsNullOrEmpty(request))
                return "ERROR: request is null or empty";

            if ((errmsg = ExtractHTTPdata(request, out Dictionary<string, string> httpDataD)) != "")
                return errmsg;

            if ((errmsg = TurnHttpDataToParameterD(httpDataD, out Dictionary<string, string> parameterD)) != "")
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
            string errmsg;
            string sqlStatement;
            switch (path)
            {
                case "/users":
                    sqlStatement = "INSERT INTO players (username, password, coinpurse) VALUES (@username, @password, 20);";
                    if (_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR:"))
                    {
                        number = 409;
                        return "USER ALREADY EXISTS";
                    }
                    number = 201;
                    return "SUCCESS";

                case "/tradings":
                    if (!doTokenCheckIn(parameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }
                    sqlStatement = "INSERT INTO trades (tradeID, cardID, username, mindamage) VALUES (@tradeID, @cardID, @Tokenname, @minDamage);";
                    return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/battles":
                    if (doTokenCheckIn(parameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }
                    
                    if ((errmsg = _DatabaseHandler.GetDeckForBattle(parameterD["Tokenname"], out List<Card> deck)).Contains("ERROR"))
                    {
                        return errmsg;
                    }

                    User player = new User(parameterD["Tokenname"], deck);
                    AddPlayer(player);
                    Battle currentBattle = WaitForOpponent();
                    string winner = currentBattle.StartBattle();
                    
                    sqlStatement = $"UPDATE scoreboard SET amountofwins = amountofwins + 1 WHERE username = {winner};";
                    _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                    return currentBattle.BattleLog;

                case "/transactions/packages":

                    if (doTokenCheckIn(parameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    if (_DatabaseHandler.IsTableEmpty("package"))
                    {
                        number = 404;
                        return "SORRY, WE HAVE RAN OUT OF CARD PACKAGES!";
                    }

                    sqlStatement = $"SELECT coinpurse FROM players WHERE username = @Tokenname";
                    if (!(int.TryParse(_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD), out int coins) &&
                        coins > 5                                                                        ))
                    {
                        number = 403;
                        return "NOT ENOUGH MONEY";
                    }

                    sqlStatement = $"UPDATE players SET coinpurse = coinpurse - 5 WHERE username = @Tokenname;";
                    number = 404;

                    return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/packages":
                    if (doTokenCheckIn(parameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    if ((parameterD["Tokenname"] != "admin"))
                    {
                        number = 403;
                        return "You don't have the privileges for this action";
                    }


                    sqlStatement = "INSERT INTO packages (id, cardasjson1, cardasjson2, cardasjson3, cardasjson4, cardasjson5) VALUES (@id, @cardasjson1, @cardasjson2, @cardasjson3, @cardasjson4, @cardasjson5);";

                    if((errmsg = _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD)).Contains("ERROR"))
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
                            currentCardD.Add(key, ExtractCardIDOutOfJsonString(item.Value, "id"));
                            currentCardD.Add("cardasjson", item.Value);

                            if (_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR"))
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
                        number = 201;
                        return "SUCCESS";
                    }


                case "/sessions":
                    sqlStatement = "SELECT COUNT(*) FROM players WHERE username = @username AND password = @password;";
                    Console.WriteLine("/sessions detected!");
                    if (int.TryParse(_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD), out int result) &&
                        result > 0                                                                                       )
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
                        if (doTokenCheckIn(parameterD))
                        {
                            number = 401;
                            return "INVALID TOKEN";
                        }
                        string tradeID = GetDataFromPath(path);
                        sqlStatement = "";
                        return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                    }
                    else 
                        return "Unknown path For POST HTTP-method";
            }
        }
        
        private string HandleGETRequest(string path, Dictionary<string, string> parameterD, ref int number)
        {
            
            if (doTokenCheckIn(parameterD))
            {
                number = 401;
                return "INVALID TOKEN";
            }
            string message;
            string sqlStatement;
            switch (path)
            {
                case "/tradings":
                    sqlStatement = "SELECT * FROM trades WHERE username != @Tokenname;";
                    string query;
                    if ((query = _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD)).Contains("ERROR"))
                        return query;
                    if (query == "JSON couldn't be serialized") 
                        number = 204;
                    return query;

                case "/scoreboard":
                    sqlStatement = "SELECT * FROM scoreboard ORDER BY amountofwins DESC;";
                    return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/stats":
                    sqlStatement = "SELECT * FROM scoreboard WHERE username = @Tokenname;";
                    return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/deck?format=plain":
                    sqlStatement = "SELECT * FROM scoreboard WHERE username = @Tokenname;";
                    return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/deck":
                    sqlStatement = "SELECT cardID1, cardID2, cardID3, cardID4 FROM decks WHERE username = @Tokenname;";
                    return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                case "/cards":
                    sqlStatement = "SELECT c.cardAsJson FROM cardcompendium cc, cards c WHERE  c.cardID = cc.cardID AND username = @Tokenname;";
                    return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                default:
                    if (path.Contains("/users/"))
                    {
                        string currentUser = GetDataFromPath(path);
                        if (parameterD["Tokenname"] == currentUser ||
                            parameterD["Tokenname"] == "admin"     )
                        {
                            parameterD["Username"] = currentUser;
                            sqlStatement = "SELECT * FROM players WHERE username = @Username;";

                            number = 401;
                            if ((message = _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD)).Contains("ERROR"))
                                number = 404;
                            return message;
                        }
                        else
                        {
                            number = 401;
                            return $"You don't have permissions retrieve userdata of {currentUser}!";
                        }
                    }
                    else
                        return "Unknown path For GET HTTP-method";
            }
        }
        
        private string HandlePUTRequest(string path, Dictionary<string, string> parameterD, ref int number)
        {
            string message;
            string sqlStatement;
            if (path == "/deck")
            {
                if (doTokenCheckIn(parameterD))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }

                int i = 1;
                foreach (var item in parameterD)
                {
                    if (item.Key != "Tokenname")
                    {
                        sqlStatement = $"SELECT amount FROM decks WHERE cardID = @cardID{i} AND username = @Tokenname;";

                        if (_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR"))
                        {
                            number = 403;
                            return "";
                        }
                        ++i;
                    }
                }

                sqlStatement = "SELECT Count(*) FROM decks WHERE username = @Tokenname;"; 
                if (_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD) == "0")
                    sqlStatement = "INSERT INTO decks (username, cardID1, cardID2, cardID3, cardID4) VALUES (@Tokenname, @cardid1, @cardid2, @cardid3, @cardid4);";
                else
                    sqlStatement = "UPDATE decks SET cardID1 = @cardid1, cardID2 = @cardid2, cardID3 = @cardid3, cardID4 = @cardid4 WHERE username = @Tokenname;";
                if (_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD).Contains("ERROR"))
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
                sqlStatement = "SELECT COUNT(*) FROM players WHERE username = @oldusername;";
                if(_DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD) == "0")
                {
                    number = 404;
                    return "No User with given name exists";
                }

                sqlStatement = "UPDATE players SET username = @username, password = @password, bio = @bio, image = @image WHERE username = @oldusername;";
                if ((message = _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD)).Contains("ERROR"))
                    return message;
                number = 200;
                return "SUCCESS";    
            }
            else return "Unknown path for PUT HTTP-method";
        }
        private string HandleDELETERequest(string path, Dictionary<string, string> parameterD, ref int number)
        {
            if (path.Contains("/tradings/"))
            {
                if (doTokenCheckIn(parameterD))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }
                string id = GetDataFromPath(path);
                string sqlStatement = $"DELETE FROM trades WHERE tradeID = {id};";
                return _DatabaseHandler.ExecuteSQLCodeSanitized(sqlStatement, parameterD);
            }   
            else return "Unknown path for DELETE HTTP-method";
        }
        // JSON HANDLER FUNCTIONS:

        public static string TurnHttpDataToParameterD(Dictionary<string, string> httpDataD, out Dictionary<string, string> parameterD)
        {
            parameterD = new Dictionary<string, string>();

            if (!httpDataD.TryGetValue("JSON-Data", out string jsonAsString))
                return "ERROR: No Key named JSON-Data was found in httpDataD";

            try
            {
                if (jsonAsString.Contains("["))
                {
                    JArray jsonData = (JArray) JsonConvert.DeserializeObject(jsonAsString);
                    
                    if ((parameterD = jsonArrayToDictionary(jsonData, httpDataD["path"])) == null)
                        return "ERROR: the parameters of the provided JSON couldn't be parsed";
                }
                else
                {
                    JObject jsonData = (JObject) JsonConvert.DeserializeObject(jsonAsString);

                    if ((parameterD = jsonObjectToDictionary(jsonData)) == null)
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

        public static Dictionary<string, string> jsonObjectToDictionary(JObject jsonData)
        {
            var parameterD = new Dictionary<string, string>();
            foreach (var property in jsonData.Properties())
                parameterD.Add(property.Name.ToLower(), property.Value.ToString());

            return parameterD;
        }
        /// <summary>
        /// option should either be the current path defined in the api-spec or "" if it is a normal jsonarray
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static Dictionary<string, string> jsonArrayToDictionary(JArray jsonArray, string option)
        {
            var outputD = new Dictionary<string, string>();
            
            switch (option)
            {
                case "/packages":
                    int index = 1;
                    outputD.Add("id", jsonArray.First().ToString());
                    jsonArray.RemoveAt(index);
                    foreach (JObject element in jsonArray)
                    {
                        outputD.Add("cardasjson" + index++, element.ToString());
                    }
                    break;

                case "/deck":
                    for (int i = 0; i < jsonArray.Count; i++)
                    {
                        outputD.Add("cardID" + (i + 1), jsonArray[i].ToString());
                    }
                    break;

                default:
                    foreach (var propRow in from JObject jsonObject in jsonArray.Children()
                                            from JProperty propRow in jsonObject.Children()
                                            select propRow)
                    {
                        outputD.Add(propRow.Name.ToString(), propRow.Value.ToString());
                    }
                    break;

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
                Battle battle = new Battle(players[1], players[2]);
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
        public static string GetDataFromPath(string path)
        {
            return path[(path.LastIndexOf('/') + 1)..(path.Length - 1)];
        }
        public static string ExtractCardIDOutOfJsonString(string jsonString, string key)
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

                // Underneath indexHelper is used to save the length of Json data
                for (int i = indexHelper + 16;
                         i < request.Length &&
                         request[i] != (char)13; i++)
                {
                    lengthAsString += request[i];
                }
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
