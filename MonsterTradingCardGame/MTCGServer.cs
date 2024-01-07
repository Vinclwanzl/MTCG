using System;
using System.Text;
using System.Net;
using System.Text.Json;

namespace MonsterTradingCardGame
{
    using System;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Security.AccessControl;
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

        private static object _DataBaseLock = new object();
        private NpgsqlConnection? _dbConnection;

        private static object _TokenLock = new object();
        private static Dictionary<string, DateTime> _userTokensD = new Dictionary<string, DateTime>();
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

            string connectionString = $"Host={dbIPAddress}:{dbPortNumber};Username={username};Password={password};Database=mtcgdb";

            lock (_DataBaseLock)
            {
                _dbConnection = new NpgsqlConnection(connectionString);

                // test the database connection 
                try
                {
                    _dbConnection.Open();
                    Console.WriteLine("Database connection was successfully established");
                    return true;
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
                if (_dbConnection != null)
                {
                    try
                    {
                        _dbConnection.Open();
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCommand, _dbConnection))
                        {
                            Console.WriteLine("\n Commmand: " + sqlCommand + "\n");
                            foreach (var parameter in parameterD)
                            {
                                cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                                //Console.WriteLine(parameter.Key + " : " + parameter.Value);
                            }
                            if (sqlCommand.Contains("SELECT"))
                            {
                                using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(cmd))
                                {
                                    DataSet dataSet = new DataSet();

                                    dataAdapter.Fill(dataSet);

                                    string jsonString = JsonConvert.SerializeObject(dataSet, Formatting.Indented);
                                    return dataAdapter.ToString();
                                }
                            }
                            else
                                return "" + cmd.ExecuteNonQuery();

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR: occured while inserting data:\n{ex.Message}");
                        return ex.Message;
                    }
                    finally
                    {
                        _dbConnection.Close();
                    }
                }
            }
            Console.WriteLine("ERROR: Database Connection has not been established successfully");
            return "We are facing database issues right now";
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


                for (int i = 0; i < request.Length; i++)
                {
                    if (request[i] == ' ')
                        Console.WriteLine(i + ":\t' '--> " + (int)request[i]);
                    else
                        Console.WriteLine(i + ":\t" + request[i] + " --> " + (int)request[i]);
                }

                Console.WriteLine($"Received request:\n{request}");

                string response = ProcessRequest(request);
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
        private string ProcessRequest(string request)
        {
            string errmsg;

            if (string.IsNullOrEmpty(request))
                return "ERROR: request is null or empty";

            if ((errmsg = ExtractHTTPdata(request, out Dictionary<string, string> httpDataD)) != "")
                return errmsg;

            // serialization test bellow (ignore)
            /*Spell spell = new("hallo -id", EDinoTypes.TERRESTRIAL, "Glas Wasser", "Wasserglas", 20, 2000);
            string jsonString = JsonConvert.SerializeObject(spell);
            Console.WriteLine(jsonString);*/

            Dictionary<string, string> Dparameters = new();
            if (httpDataD.TryGetValue("JSON-Data", out string jsonAsString))
            {
                if (jsonAsString.Contains("["))
                {
                    JArray jsonData;
                    // this check is probably overkill
                    if ((jsonData = (JArray) JsonConvert.DeserializeObject(jsonAsString)) == null)
                        return "ERROR: the provided JSON couldn't be deserialized, please check the Syntax";

                    if ((Dparameters = GetParametersFromJsonArray(jsonData, httpDataD["path"])) == null)
                        return "ERROR: the parameters of the provided JSON couldn't be parsed";
                }
                else
                {
                    JObject jsonData; 
                    // this check is probably overkill
                    if ((jsonData = (JObject) JsonConvert.DeserializeObject(jsonAsString)) == null)
                        return "ERROR: the provided JSON couldn't be deserialized, please check the Syntax";

                    if ((Dparameters = GetParametersFromJsonObject(jsonData)) == null)
                        return "ERROR: the parameters of the provided JSON couldn't be parsed";
                }
            }

            switch (httpDataD["HTTP-method"])
            {
                case "POST":
                    return HandlePOSTRequest(httpDataD["path"], Dparameters);
                case "GET":
                    return HandleGETRequest(httpDataD["path"], Dparameters);
                case "PUT":
                    return HandlePUTRequest(httpDataD["path"], Dparameters);
                case "DELETE":
                    return HandleDELETERequest(httpDataD["path"], Dparameters);
                default:
                    return "ERROR: HTTPMETHOD is not in the api spec";
            }
        }

        private string HandlePOSTRequest(string path, Dictionary<string, string> parameterD)
        {
            string sqlStatement;
            switch (path)
            {
                case $"/users":
                    sqlStatement = "INSERT INTO player (username, password, coinpurse) VALUES (@username, @password, 20);";
                    return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                case "/tradings":
                    if (IsTokenActive(parameterD["username"]))
                    {
                        sqlStatement = "INSERT INTO trade (tradeID, cardID, username, mindamage)\r\nVALUES (@tradeID, @cardID, @username, @minDamage);";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                    }
                    else
                        return "401";
                case "/battles":
                    // gaaaaanz wichtiges TODO
                    if (IsTokenActive(parameterD["username"]))
                    {
                        sqlStatement = "SELECT * FROM PLAYER";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                    }
                    else
                        return "401";
                case "/transactions/packages":
                    if (IsTokenActive(parameterD["username"]))
                    {
                        sqlStatement = $"SELECT coinpurse FROM player WHERE username = @username";
                        if (int.TryParse(ExecuteSQLCodeSanitized(sqlStatement, parameterD), out int coins) &&
                            coins > 5)
                        {
                            sqlStatement = "SELECT COUNT(*) FROM package;";
                            if (int.TryParse(ExecuteSQLCodeSanitized(sqlStatement, parameterD), out int amountOfPackages) &&
                               amountOfPackages > 0)
                            {
                                coins -= 5;
                                sqlStatement = $"UPDATE player SET coinpurse = {coins} WHERE username = @username;";
                                ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                                return "200";
                            }
                            else
                                return "404";
                        }
                        else
                            return "403";
                    }
                    else
                        return "401";
                case "/packages":
                    if (!IsTokenActive(parameterD["username"]))
                        return "401";
                    
                    if (!(parameterD["username"] == "admin"))
                        return "403";

                    bool cardAlreadyExisted = false;
                    sqlStatement = "INSERT INTO package (cardAsJson1, cardAsJson2, cardAsJson3, cardAsJson4, cardAsJson5) VALUES (@cardasjson1, @cardasjson2, @cardasjson3, @cardasjson4, @cardasjson5);";

                    ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                    
                    
                    sqlStatement = "INSERT INTO package (cardAsJson1, cardAsJson2, cardAsJson3, cardAsJson4, cardAsJson5) VALUES (@cardasjson1, @cardasjson2, @cardasjson3, @cardasjson4, @cardasjson5);";
                    
                    // TODO: place cards into database

                    if (cardAlreadyExisted)
                        return "409";
                    else
                        return "201";
                    

                case "/sessions":
                    sqlStatement = "SELECT COUNT(*) FROM player WHERE username = @username AND password = @password;";

                    if (int.TryParse(ExecuteSQLCodeSanitized(sqlStatement, parameterD), out int result) &&
                        result > 0                                                                       )
                    {
                        SetToken(parameterD["username"]);
                        return "200";
                    }
                    else
                    {
                        return "401";
                    }

                default:
                    if(path.Contains("/tradings/"))
                    {
                        string tradeID = GetDynamicDataFromPath(path);
                        sqlStatement = "";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                    }
                    else return "Unknown path For POST HTTP-method";
            }
        }
        private string HandleGETRequest(string path, Dictionary<string, string> parameterD)
        {
            if (IsTokenActive(parameterD["username"]))
            {
                string sqlStatement;
                switch (path)
                {
                    case "/tradings":
                        sqlStatement = "SELECT * FROM trade;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/scoreboard":
                        // TODO: get username from username
                        sqlStatement = "SELECT * FROM scoreboard ORDER BY amountofwins DESC;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/stats":
                        // TODO: get username from username
                        sqlStatement = "SELECT * FROM scoreboard WHERE username = @username;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/deck?format=plain":
                        // TODO: get username from username
                        sqlStatement = "SELECT * FROM scoreboard WHERE username = @username;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/deck":
                        // TODO: get username from username
                        sqlStatement = "SELECT cardID1, cardID2, cardID3, cardID4 FROM deck WHERE username = @username;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    case "/cards":
                        // TODO: get username from username
                        sqlStatement = "SELECT c.cardAsJson FROM cardcompendium cc, cards c WHERE  c.cardID = cc.cardID AND username = @username;";
                        return ExecuteSQLCodeSanitized(sqlStatement, parameterD);

                    default:
                        if (path.Contains("/users/"))
                        {
                            
                            parameterD.Add("username", GetDynamicDataFromPath(path));
                            sqlStatement = "SELECT * FROM player WHERE username = @username;";
                            return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                            
                        }
                        else
                            return "Unknown path For GET HTTP-method";
                }
            }
            else
                return "Unknown path For GET HTTP-method";
        }
        private string HandlePUTRequest(string path, Dictionary<string, string> parameterD)
        {
            string sqlStatement;
            if (path == "/deck")
            {
                // TODO: get username from username
                sqlStatement = "UPDATE deck SET cardID1 = @cardid1, cardID2 = @cardid2, cardID3 = @cardid3, cardID4 = @CardID4 WHERE username = @username;";
                return ExecuteSQLCodeSanitized(sqlStatement, parameterD); 
            }
            else if (path.Contains("/users/"))
            {
                string username = GetDynamicDataFromPath(path);
                sqlStatement = $"UPDATE player SET password = @password, bio = @bio, image = @image WHERE username = {username};";
                return ExecuteSQLCodeSanitized(sqlStatement, parameterD);    
            }
            else return "Unknown path for PUT HTTP-method";
        }
        private string HandleDELETERequest(string path, Dictionary<string, string> parameterD)
        {
            if (path.Contains("/tradings/"))
            {
                if (IsTokenActive(parameterD["username"]))
                {
                    string id = GetDynamicDataFromPath(path);
                    string sqlStatement = $"DELETE FROM trade WHERE tradeID = {id};";
                    return ExecuteSQLCodeSanitized(sqlStatement, parameterD);
                }
                else
                    return "401";
            }   
            else return "Unknown path for DELETE HTTP-method";
        }
        // JSON HANDLER FUNCTIONS:
        public Dictionary<string, string> GetParametersFromJsonObject(JObject jsonData)
        {
            var parameterD = new Dictionary<string, string>();
            foreach (var property in jsonData.Properties())
            {
                parameterD.Add(property.Name.ToLower(), property.Value.ToString());
            }

            return parameterD;
        }
        public Dictionary<string, string> GetParametersFromJsonArray(JArray jsonArray, string path)
        {
            var parameterD = new Dictionary<string, string>();
            if (path == "/packages")
            {
                int index = 1;
                foreach (JObject element in jsonArray)
                {
                    parameterD.Add("cardasjson" + index++, element.ToString());
                }
            }
            else
            {
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    parameterD.Add("cardID"+(i+1), jsonArray[i].ToString());
                }
            }

            return parameterD;
        }

        // STRING PARSING FUNCTIONS:
        public static string GetDynamicDataFromPath(string path)
        {
            return path[(path.LastIndexOf('/') + 1)..(path.Length - 1)];
        }

        public string ExtractHTTPdata(string request, out Dictionary<string, string> httpDataD)
        {
            Dictionary<string, string> outputD = new Dictionary<string, string>();
            httpDataD = outputD;
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
            outputD.Add("HTTP-method", httpMethod);
            outputD.Add("path", path);
            httpDataD = outputD;

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
                    if (request[i] != '-')
                        minusHasBeenReached = true;
                    if(minusHasBeenReached)
                        restOftheToken += request[i];
                    else
                        username += request[i];
                }
                if(restOftheToken == "-mtcgToken")
                    outputD.Add("username", username);
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

                outputD.Add("JSON-Data", $"{request[(1 + request.Length - (indexHelper))..(request.Length - 1)].Replace("\\", string.Empty)}");
            }
            httpDataD = outputD;
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
