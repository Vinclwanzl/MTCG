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
    using System.Net.Sockets;
    using System.Security.AccessControl;
    using Newtonsoft.Json;
    class MTCGServer
    {
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        
        public async Task StartServerAsync(string ipAddress, int portNumber)
        {
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            serverSocket.Bind(
                new IPEndPoint(
                    IPAddress.Parse(ipAddress),
                    portNumber
                )
            );
            serverSocket.Listen(10);

            Console.WriteLine("Server started...");

            while (true)
            {
                Socket clientSocket = await serverSocket.AcceptAsync();
                //Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine("Client connected.");

                // Use a thread pool or async method to handle each client
                _ = Task.Run(() => HandleClientAsync(clientSocket));
            }
        }

        static async Task HandleClientAsync(Socket clientSocket)
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


                Console.WriteLine($"Received JSON request:\n{request}");

                // Parse JSON using JSON.NET
                // JsonObject requestModel = JsonConvert.DeserializeObject<JsonObject>(requestJson);

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

        static string ProcessRequest(string request)
        {
            if (request == null || request == "")
                return "ERROR: request input is null or empty";

            string httpMethod = "", path = "", response = "";
            ExtractHTTPMethodAndPath(request, ref httpMethod, ref path);

            JsonObject jsonData = new();

            if (request.Contains("Content-Type: application/json"))
            { 
                if(!(response = ExtractJsonData(request)).Contains("ERROR"))
                    jsonData = JsonConvert.DeserializeObject<JsonObject>(ExtractJsonData(response));
                else
                    Console.WriteLine(response);
                response = "";
            }

            switch (httpMethod+path)
            {
                case "POST/users":
                    response = HandlePOSTRequest(path);
                    break;

                case $"GET/users/"/*{username}*/:
                    response = HandlePOSTRequest(path);
                    break;

                case $"PUT/users/"/*{username}*/:
                    response = HandlePOSTRequest(path);
                    break;

                case "POST/sessions":
                    response = HandlePOSTRequest(path);
                    break;

                case "POST/packages":
                    response = HandlePOSTRequest(path);
                    break;

                case "POST/transactions/packages":
                    response = HandlePOSTRequest(path);
                    break;

                case "GET/cards":
                    response = HandlePOSTRequest(path);
                    break;

                case "GET/deck":
                    response = HandlePOSTRequest(path);
                    break;

                case "PUT/deck":
                    response = HandlePOSTRequest(path);
                    break;

                case "GET/deck?format=plain":
                    response = HandlePOSTRequest(path);
                    break;

                case "GET/stats":
                    response = HandlePOSTRequest(path);
                    break;

                case "GET/scoreboard":
                    response = HandlePOSTRequest(path);
                    break;

                case "POST/battles":
                    response = HandlePOSTRequest(path);
                    break;

                case "GET/tradings":
                    response = HandlePOSTRequest(path);
                    break;

                case "POST/tradings":
                    response = HandlePOSTRequest(path);
                    break;

                case $"DELETE/tradings/"/*{id}*/:
                    response = HandlePOSTRequest(path);
                    break;

                case $"POST/tradings/"/*{id}"*/:
                    response = HandlePOSTRequest(path);
                    break;

                default:
                    // TODO: if cases with dynmatic values don't work make ifelseifelseifelse spaggethi here
                    response = "Unhandled request";
                    break;
            }
            return response;
        }
        private static string HandlePOSTRequest(string path)
        {

            // TODO figure out if that is the pattern I want
            return "POST bekommen :^)";
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
                     request[i] != 13; i++)
            
                lengthAsString += request[i];
            
            // Underneath lengthHelper is used to save the length of Json data
            if (!int.TryParse(lengthAsString, out lengthHelper))
                return "PARSING-ERROR: Could not Parse Content-Length";

            return request[(request.Length - (1 + lengthHelper))..(request.Length - 1)];
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
        public class JsonObject
        {
            //public string Method { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            // Add other properties as needed
        }
    }
}
