using System;
using System.Text;
using System.Net;
using System.Text.Json;

namespace MonsterTradingCardGame
{
    using System;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Security.AccessControl;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Npgsql;

    class MTCGServer
    {
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private NpgsqlConnection? _dbConnection;
        private static readonly object _DatabaseLock = new object();

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

        private bool EstablishConnection(string dbIPAddress, int dbPortNumber)
        {
            Console.WriteLine("Enter the following credentials to connect to the database");
            Console.WriteLine("The Username of the db Host: ");
            string username = Console.ReadLine();
            Console.WriteLine("The Password of the db Host: ");
            string password = Console.ReadLine();

            string connectionString = $"Host={dbIPAddress}:{dbPortNumber};Username={username};Password={password};Database=mtcgdb";

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
        private bool ExecuteQuerySafely(string sqlCommand, string[] parameterKeys, string[] parameterValues)
        {
            if (_dbConnection != null)
            {
                try
                {
                    _dbConnection.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCommand, _dbConnection))
                    {
                        for (int i = 0; i < parameterKeys.Length; i++)
                        {
                            cmd.Parameters.AddWithValue(parameterKeys[i], parameterValues[i]);
                        }
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Process the results
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR occured while executing query:\n{ex.Message}");
                    return false;
                }
                finally
                {
                    _dbConnection.Close();
                }
            }
            Console.WriteLine("ERROR: Database Connection has not been established successfully");
            return false;
        }

        private async Task HandleClientAsync(Socket clientSocket)
        {
            try
            {
                await semaphoreSlim.WaitAsync();

                byte[] buffer = new byte[1024];
                int length = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                string request = Encoding.UTF8.GetString(buffer, 0, length);


                /*for (int i = 0; i < request.Length; i++)
                {
                    if (request[i] == ' ')
                        Console.WriteLine(i + ":\t' '--> " + (int)request[i]);
                    else
                        Console.WriteLine(i + ":\t" + request[i] + " --> " + (int)request[i]);
                }*/


                // Console.WriteLine($"Received JSON request:\n{request}");

                // Process the request
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

        private string ProcessRequest(string request)
        {
            if (request == null || request == "")
                return "ERROR: request input is null or empty";

            string httpMethod = "", path = "", response = "";
            ExtractHTTPMethodAndPath(request, ref httpMethod, ref path);

            if (!CheckStringForEmptiness(httpMethod))
                return "Could not extract HTTP method";
            if (!CheckStringForEmptiness(path))
                return "Could not extract path";

            // serialization test bellow (ignore)
            /*Spell spell = new("hallo -id", EDinoTypes.TERRESTRIAL, "Glas Wasser", "Wasserglas", 20, 2000);
            string jsonString = JsonConvert.SerializeObject(spell);
            Console.WriteLine(jsonString);*/

            string jsonAsString = "";
            if (request.Contains("Content-Type: application/json")        &&
                (jsonAsString = ExtractJsonData(request)).Contains("ERROR"))
            {
                Console.WriteLine(jsonAsString);
                return "The supplied Json had caused an ERROR, please check your JSON input";
            }
            switch (httpMethod)
            {
                case "POST":
                    response = HandlePOSTRequest(path, jsonAsString);
                    break;

                case "GET":
                    response = HandleGETRequest(path, jsonAsString);
                    break;

                case "PUT":
                    response = HandlePUTRequest(path, jsonAsString);
                    break;
                case "DELETE":
                    response = HandleDELETERequest(path, jsonAsString);
                    break;
                default:
                    response =  "HTTPMETHOD is not in the api spec";
                    break;
            }
            return response;
        }
        private static string HandlePOSTRequest(string path, string jsonAsString)
        {
            switch (path)
            {
                case $"/users":
                    return "201";
                case "/tradings":
                    return "";
                case "/battles":
                    return "";
                case "/transactions/packages":
                    return "";
                case "/packages":
                    return "";
                case "/sessions":
                    return "";
                default:
                    if(path.Contains("/tradings/"))
                    {
                        string tradeID = GetDynamicInputFromPath(path);
                        return "";
                    }
                    else return "Unknown path For POST HTTP-method";
            }
        }
        private string HandleGETRequest(string path, string jsonAsString)
        {
            switch (path)
            {
                case "/tradings":

                    return "";

                case "/scoreboard":

                    return "";

                case "/stats":

                    return "";

                case "/deck?format=plain":

                    return "";

                case "/deck":

                    return "";

                case "/cards":

                    return "";

                default:
                    if (path.Contains("/users/"))
                    {
                        string username = GetDynamicInputFromPath(path);
                        return "";
                    }
                    else return "Unknown path For GET HTTP-method";
            }
        }
        private string HandlePUTRequest(string path, string jsonAsString)
        {
            if (path == "/deck")
            {
                return "";
            }
            else if (path.Contains("/users/"))
            {
                string username = GetDynamicInputFromPath(path);
                return "";
            }
            else return "Unknown path for PUT HTTP-method";
        }
        private string HandleDELETERequest(string path, string jsonAsString)
        {
            if (path.Contains("/tradings/"))
            {
                string id = GetDynamicInputFromPath(path);
                return "200";
            } 
            else return "Unknown path for DELETE HTTP-method";
        }
        private static string GetDynamicInputFromPath(string path)
        {
            return path[(path.LastIndexOf('/') + 1) .. (path.Length - 1)];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns json data as string and an ERROR-message otherwise
        private static string ExtractJsonData(string request) 
        {
            string lengthAsString = "";
            int lengthHelper;

            // Underneath lengthHelper is used to capture the index of "Content-Length: "
            if ((lengthHelper = request.IndexOf("Content-Length: ")) == -1)
                return "FORMATTING-ERROR: Content-Length Field was not found";

            for (int i = lengthHelper + 16;
                     i < request.Length && 
                     request[i] != (char) 13; i++)
            
                lengthAsString += request[i];
            
            // Underneath lengthHelper is used to save the length of Json data
            if (!int.TryParse(lengthAsString, out lengthHelper))
                return "PARSING-ERROR: Could not Parse Content-Length";
            Console.WriteLine($"\n{request[(2 + request.Length - ( lengthHelper))..(request.Length - 2)]/*.Replace("\\", string.Empty)*/}\n");
            return request[(request.Length - ( lengthHelper))..(request.Length - 2)]/*.Replace("\\", string.Empty)*/; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="HTTPMethod"></param>
        /// <param name="Path"></param>
        /// <returns>returns HTTPMethod and path, as strings through the ref parameters HTTPMethod and path</returns>
        private static void ExtractHTTPMethodAndPath(string request, ref string HTTPMethod, ref string Path)
        {
            int fieldIndex = 0;
            for (int i = 0; i < request.Length && fieldIndex < 3; i++)
            {
                if (request[i] == ' ')    
                    ++fieldIndex;
                else if (fieldIndex == 0) 
                    HTTPMethod += request[i];
                else if (fieldIndex == 1) 
                    Path += request[i];
            }
        }
        public static bool ValidatePORT(int input)
        {
            return (0 <= input && input <= 65535);
        }
        public static bool ValidateIP(string input)
        {
            if (!CheckStringForEmptiness(input))
                return false;
            IPAddress _;
            return IPAddress.TryParse(input, out _);
        }
        public static bool CheckStringForEmptiness(string input)
        {
            return (input != null && input != "");
        }
    }
}
