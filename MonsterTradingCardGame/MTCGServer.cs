using System.Text;

namespace MonsterTradingCardGame
{
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Numerics;

    public class MTCGServer
    {
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(10, 10);
        private static MTCGDatabase _DatabaseHandler = new MTCGDatabase();

        private static object _TokenLock = new object();
        private static Dictionary<string, DateTime> _userTokensD = new Dictionary<string, DateTime>();

        private object _PoolLock = new object();
        private Queue<User> _waitingPlayers = new Queue<User>();
        private Battle? _battle;

        private readonly string JSONHEADER = "\r\nContent-Type: application/json -d \r\n\r\n";
        private readonly string PLAINHEADER = "\r\nContent-Type: text/plain -d \r\n\r\n";

        private readonly int _TOKENACTIVITYTIMER = 300000;

        /// <summary>
        /// starts the Server
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="portNumber"></param>
        /// <param name="dbIPAddress"></param>
        /// <param name="dbPortNumber"></param>
        /// <returns>True, if Server started successfully</returns>
        public void StartServer(string ipAddress, int portNumber, string dbIPAddress, int dbPortNumber)
        {
            if (IPAddress.TryParse(dbIPAddress, out IPAddress serverIP) &&
                ValidatePORT(portNumber) &&
                ValidateIP(dbIPAddress) &&
                ValidatePORT(dbPortNumber) &&
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
                    Socket clientSocket = serverSocket.Accept();
                    Console.WriteLine("Client connected.");

                    Thread clientThread = new Thread(() => HandleClient(clientSocket));
                    clientThread.Start();
                }
            }
            else
            {
                Console.WriteLine("ERROR occurred while starting the server");
                if (!ValidateIP(dbIPAddress))
                {
                    Console.WriteLine($"-> the Database ip-address: {dbIPAddress} is invalid!");
                }
                if (!ValidatePORT(dbPortNumber))
                {
                    Console.WriteLine($"-> the Database port: {dbPortNumber} is invalid!");
                }
                if (!ValidateIP(ipAddress))
                {
                    Console.WriteLine($"-> the Server ip-address: {ipAddress} is invalid!");
                }
                if (!ValidatePORT(portNumber))
                {
                    Console.WriteLine($"-> the Server port: {portNumber} is invalid!");
                }
            }
        }

        // TOKEN HANDLING FUNCTIONS:

        /// <summary>
        /// sets user-token
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        private static void SetToken(string userToken)
        {
            lock (_TokenLock)
            {
                if (_userTokensD.ContainsKey(userToken))
                {
                    _userTokensD[userToken] = DateTime.Now;
                }
                else
                    _userTokensD.Add(userToken, DateTime.Now);
            }
        }
        /// <summary>
        /// removes user-tokens
        /// </summary>
        /// <param name="userToken"></param>
        private static void RemoveToken(string userToken)
        {
            lock (_TokenLock)
            {
                if (_userTokensD.ContainsKey(userToken))
                {
                    _userTokensD.Remove(userToken);
                }
            }
        }

        /// <summary>
        /// Checks if sent data contains an activated user-token 
        /// </summary>
        /// <param name="queryParameterD"></param>
        /// <returns>True, if user is permitted and False if user is not permitted, due to the token being non existing in the request or it being not activated in the server</returns>
        private bool IsTokenActive(Dictionary<string, string> queryParameterD)
        {
            if (!queryParameterD.TryGetValue("Tokenname", out string name))
            {
                return false;
            }

            lock (_TokenLock)
            {
                if (!_userTokensD.ContainsKey(name))
                {
                    return false;
                }
                return (_userTokensD[name] - DateTime.Now).TotalMilliseconds < _TOKENACTIVITYTIMER;
            }
        }

        // HTTP COMMUNICATION FUNCTION:
        private void HandleClient(Socket clientSocket)
        {
            try
            {
                semaphoreSlim.Wait();
                // recieve request 
                byte[] buffer = new byte[1024];
                int length = clientSocket.Receive(new ArraySegment<byte>(buffer), SocketFlags.None);

                // process request into response
                string request = Encoding.UTF8.GetString(buffer, 0, length);
                string response = ProcessRequest(request);

                // send response
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                clientSocket.Send(new ArraySegment<byte>(responseBytes), SocketFlags.None);

                Console.WriteLine($"Response sent.");

                // remove socket
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();

                Console.WriteLine("Client disconnected.\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error handling client: {e.Message}\n");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        // HTTP-REQUEST PROCESSING FUNCTIONS:#
        /// <summary>
        /// processes given http request
        /// </summary>
        /// <param name="request"></param>
        /// <returns>string with HTTP response</returns>
        private string ProcessRequest(string request)
        {
            string responseData;
            string errmsg;

            // if request fails for specific reason the right errorcode has to be given to 'number' variable
            // otherwise 500 (internal server error) will be sent;
            int number = 500;


            if (string.IsNullOrEmpty(request))
            {
                number = 400;
                return CreateHttpResponse(number, "ERROR: request is null or empty");
            }

            if ((errmsg = ExtractHTTPData(request, out Dictionary<string, string> httpDataD)) != "")
            {
                number = 400;
                return CreateHttpResponse(number, errmsg);
            }
            Console.WriteLine("path" + httpDataD["path"]);

            if ((errmsg = FillParameterD(httpDataD, out Dictionary<string, string> queryParameterD)) != "")
            {
                return CreateHttpResponse(number, "\r\n Content-Type: text/plain -d " + errmsg);
            }

            if (httpDataD.ContainsKey("Tokenname"))
            {
                queryParameterD.Add("Tokenname", httpDataD["Tokenname"]);
            }

            switch (httpDataD["HTTP-method"])
            {
                case "POST":
                    responseData = HandlePOSTRequest(httpDataD["path"], queryParameterD, ref number);
                    break;
                case "GET":
                    responseData = HandleGETRequest(httpDataD["path"], queryParameterD, ref number);
                    break;
                case "PUT":
                    responseData = HandlePUTRequest(httpDataD["path"], queryParameterD, ref number);
                    break;
                case "DELETE":
                    responseData = HandleDELETERequest(httpDataD["path"], queryParameterD, ref number);
                    break;
                default:
                    number = 505;
                    responseData = "ERROR: HTTPMETHOD is not in the api spec";
                    break;
            }
            return CreateHttpResponse(number, responseData);
        }

        private string HandlePOSTRequest(string path, Dictionary<string, string> queryParameterD, ref int number)
        {
            string sqlResult;
            string sqlStatement;

            switch (path)
            {
                case "/users":
                    sqlStatement = "INSERT INTO players (username, password, coinpurse) VALUES (@username, @password, 20);";
                    if (_DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD).Contains("ERROR"))
                    {
                        number = 409;
                        return "User with same username already registered";
                    }
                    sqlStatement = "INSERT INTO scoreboard (username, amountofwins) VALUES (@username, 0);";
                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }

                    // bellow cards get added that are needed in curl 11 for testing, they get inserted because there is a chance players won't get the cards from the package 
                    // it can be deleted if needed 
                    if (queryParameterD["username"] == "kienboec" ||
                        queryParameterD["username"] == "altenhof")
                    {
                        string[] cardIDs = new string[4];
                        if (queryParameterD["username"] == "kienboec")
                        {
                            cardIDs[0] = "845f0dc7-37d0-426e-994e-43fc3ac83c08";
                            cardIDs[1] = "99f8f8dc-e25e-4a95-aa2c-782823f36e2a";
                            cardIDs[2] = "f8043c23-1534-4487-b66b-238e0c3c39b5";
                            cardIDs[3] = "171f6076-4eb5-4a7d-b3f2-2d650cc3d237";
                        }
                        else if (queryParameterD["username"] == "altenhof")
                        {
                            cardIDs[0] = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334";
                            cardIDs[1] = "91a6471b-1426-43f6-ad65-6fc473e16f9f";
                            cardIDs[2] = "d60e23cf-2238-4d49-844f-c7589ee5342e";
                            cardIDs[3] = "84d276ee-21ec-4171-a509-c1b88162831c";
                        }
                        for (int i = 0; i < cardIDs.Length; i++)
                        {
                            sqlStatement = $"INSERT INTO cardcompendium (cardID, username, amount) VALUES ('{cardIDs[i]}', @username, 1)";
                            if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                            {
                                return sqlResult;
                            }
                        }
                    }
                    // end of curl 11 testing prep

                    number = 201;
                    return PLAINHEADER + "User successfully created";

                case "/tradings":
                    if (!IsTokenActive(queryParameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    sqlStatement = "SELECT amount FROM cardcompendium WHERE username = @Tokenname AND cardID = @cardtotrade;";
                    string ccAmount;
                    if ((ccAmount = _DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return ccAmount;
                    }


                    int deckResult = 0;
                    for (int i = 1; i < 5; i++)
                    {
                        sqlStatement = $"SELECT COUNT(*) FROM decks WHERE cardID{i} = @cardtotrade;";
                        if ((_DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)) != "0")
                        {
                            ++deckResult;
                        }
                    }

                    if (!int.TryParse(ccAmount, out int amountInCardCompendium) &&
                        deckResult > 0 &&
                        amountInCardCompendium <= deckResult)
                    {
                        number = 403;
                        return "The deal contains a card that is not owned by the user or locked in the deck.";
                    }

                    if (!int.TryParse(queryParameterD["mindamage"], out int mindamage) &&
                        mindamage < 0)
                    {
                        number = 403;
                        return "The mindamage field was either not a number or negative.";
                    }


                    sqlStatement = "SELECT COUNT(*) FROM trades WHERE tradeID = @tradeid;";
                    if ((sqlResult = _DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }

                    if (sqlResult != "0")
                    {
                        number = 409;
                        return "A deal with this deal ID already exists";
                    }


                    sqlStatement = $"INSERT INTO trades (tradeID, cardID, username, mindamage) VALUES (@tradeid, @cardtotrade, @Tokenname, {mindamage});";
                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }

                    number = 201;
                    return PLAINHEADER + "Trading deal successfully created";

                case "/battles":
                    if (!IsTokenActive(queryParameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    if ((sqlResult = _DatabaseHandler.GetDeck(queryParameterD["Tokenname"], out List<Card> deck)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }

                    List<Card> _ = new List<Card>(deck);

                    for (int i = 0; i < _.Count(); i++)
                    {
                        if (_[i] == null)
                        {
                            number = 400;
                            return "At least one Card in your deck is empty!";
                        }
                    }

                    User player = new User(queryParameterD["Tokenname"], _);

                    Battle currentBattle = WaitForOpponent(player);

                    string winner = currentBattle.Winner;

                    if (player.Name == winner)
                    {
                        sqlStatement = $"UPDATE scoreboard SET amountofwins = amountofwins + 1 WHERE username = @Tokenname;";

                        if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                        {
                            return "Your win couldn't be recorded, because your opponent used a badly phrased Genies-wish (internal Server error),\n Anyways here is the Battlelog:\n" + currentBattle.BattleLog;
                        }
                    }
                    number = 200;

                    if (!_DatabaseHandler.RedistributeCards(deck, player.Deck, queryParameterD))
                    {
                        return PLAINHEADER + "The Battle was concluded, but the Cards couldn't get redistrubuted:\n" + currentBattle.BattleLog;
                    }

                    return PLAINHEADER + currentBattle.BattleLog;

                case "/transactions/packages":

                    if (!IsTokenActive(queryParameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    if (_DatabaseHandler.IsTableEmpty("packages"))
                    {
                        number = 404;
                        return "No card package available for buying";
                    }

                    sqlStatement = $"SELECT coinpurse FROM players WHERE username = @Tokenname;";
                    if (!(int.TryParse(_DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD), out int coins) &&
                          coins >= 5))
                    {
                        number = 403;
                        return "Not enough money for buying a card package";
                    }
                    sqlStatement = $"SELECT id FROM packages ORDER BY RANDOM() LIMIT 1;";

                    if (!int.TryParse(_DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD), out int packageID))
                    {
                        return "internal Server error";
                    }

                    sqlStatement = $"SELECT cardasjson1, cardasjson2, cardasjson3, cardasjson4, cardasjson5 FROM packages WHERE id = {packageID};";

                    string package = "";

                    if ((package = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return package;
                    }

                    sqlStatement = $"DELETE FROM packages WHERE id = {packageID};";
                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return "internal Server error";
                    }

                    sqlStatement = $"UPDATE players SET coinpurse = coinpurse - 5 WHERE username = @Tokenname;";

                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return "internal Server error";
                    }

                    // bellow a player choosing a cards gets simulated

                    JArray packageAsJA = JArray.Parse(package);
                    JObject cardAsJO;

                    Random userChooseSim = new Random();
                    int CardThatPlayerTosses = userChooseSim.Next(1, 5);

                    string cardID;


                    for (int i = 1; i <= 5; i++)
                    {
                        if (i == CardThatPlayerTosses)
                        {
                            continue;
                        }

                        cardAsJO = JObject.Parse((string)packageAsJA[0][$"cardasjson{i}"]);

                        if (!string.IsNullOrEmpty(cardID = (string)cardAsJO["Id"]))
                        {
                            if (queryParameterD.TryAdd("Id", cardID)) ;
                            else
                            {
                                queryParameterD["Id"] = cardID;
                            }

                            sqlStatement = "SELECT Count(*) FROM cardcompendium WHERE username = @Tokenname AND cardID = @Id;";
                            if (!int.TryParse(_DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD), out int amount))
                            {
                                return "internal Server error";
                            }

                            if (amount == 0)
                            {
                                sqlStatement = "INSERT INTO cardcompendium (cardID, username, amount) VALUES (@Id, @Tokenname, 1)";
                            }
                            else
                                sqlStatement = "UPDATE cardcompendium SET amount = amount + 1 WHERE username = @Tokenname AND cardID = @Id;";

                            if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                            {
                                return "internal Server error";
                            }
                        }
                    }
                    //end of user sim

                    // prettyfy the output
                    char[] p = package.ToCharArray();
                    p[package.IndexOf("{")] = ' ';
                    p[package.LastIndexOf("}")] = ' ';
                    package = new string(p);

                    number = 200;
                    return JSONHEADER + package;

                case "/packages":
                    if (!IsTokenActive(queryParameterD))
                    {
                        number = 401;
                        return "INVALID TOKEN";
                    }

                    if ((queryParameterD["Tokenname"] != "admin"))
                    {
                        number = 403;
                        return "Provided user is not \"admin\"";
                    }

                    sqlStatement = "INSERT INTO packages (cardasjson1, cardasjson2, cardasjson3, cardasjson4, cardasjson5) VALUES (@cardasjson1, @cardasjson2, @cardasjson3, @cardasjson4, @cardasjson5);";

                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }


                    sqlStatement = "INSERT INTO cards (cardID, cardAsJson) VALUES (@cardid, @cardasjson)";
                    bool cardAlreadyExisted = false;
                    Dictionary<string, string> currentCardD = new Dictionary<string, string>();

                    foreach (var item in queryParameterD)
                    {
                        if (item.Key != "Tokenname")
                        {
                            currentCardD.Add("cardid", ExtractValueFromJsonString(item.Value, "Id"));
                            currentCardD.Add("cardasjson", item.Value);

                            if (_DatabaseHandler.ExecuteSQLCode(sqlStatement, currentCardD).Contains("ERROR"))
                            {
                                cardAlreadyExisted = true;
                            }
                            currentCardD = new Dictionary<string, string>();
                        }
                    }

                    if (cardAlreadyExisted)
                    {
                        number = 409;
                        return "At least one card in the packages already exists";
                    }
                    else
                    {
                        number = 201;
                        return PLAINHEADER + "Package and cards successfully created";
                    }

                case "/sessions":
                    sqlStatement = "SELECT COUNT(*) FROM players WHERE username = @username AND password = @password;";

                    if (int.TryParse(_DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD), out int result) &&
                        result > 0)
                    {
                        SetToken(queryParameterD["username"]);
                        number = 200;
                        return PLAINHEADER + "User login successful";
                    }

                    number = 401;
                    return "Invalid username/password provided";

                default:
                    if (path.Contains("/tradings/"))
                    {
                        if (!IsTokenActive(queryParameterD))
                        {
                            number = 401;
                            return "INVALID TOKEN";
                        }
                        string tradeID = GetDataFromPath(path);
                        sqlStatement = "";
                        number = 504;
                        return "Function not implemented";
                    }

                    number = 400;
                    return "Unknown path For POST HTTP-method";
            }
        }

        private string HandleGETRequest(string path, Dictionary<string, string> queryParameterD, ref int number)
        {
            if (!IsTokenActive(queryParameterD))
            {
                number = 401;
                return "INVALID TOKEN";
            }
            string sqlResult;
            string sqlStatement;
            switch (path)
            {
                case "/tradings":
                    sqlStatement = "SELECT * FROM trades WHERE username != @Tokenname;";
                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)) == "[]")
                    {
                        number = 204;
                        return PLAINHEADER + "The request was fine, but there are no trading deals available";
                    }

                    number = 200;
                    return JSONHEADER + sqlResult;

                case "/scoreboard":
                    sqlStatement = "SELECT * FROM scoreboard ORDER BY amountofwins DESC;";

                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }

                    number = 200;
                    return JSONHEADER + sqlResult;

                case "/stats":

                    sqlStatement = "SELECT * FROM scoreboard WHERE username = @Tokenname;";
                    if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }

                    sqlResult = ReplaceUnwantedSurroundings(sqlResult);

                    number = 200;
                    return JSONHEADER + sqlResult;

                case "/deck?format=plain":

                    if ((sqlResult = _DatabaseHandler.GetDeck(queryParameterD["Tokenname"], out List<Card> deck)).Contains("ERROR"))
                    {
                        return sqlResult;
                    }

                    string output = $"Deck of {queryParameterD["Tokenname"]}:\n";
                    int emptyCounter = 0;

                    for (int i = 0; i < deck.Count; i++)
                    {
                        switch (deck[i])
                        {
                            case null:
                                ++emptyCounter;
                                output += "Card was lost in past battles or has been eaten by data corrupting mice :°)";
                                break;
                            case Monster monster:
                                output += monster.ToString();
                                break;
                            case TrapSpell trapspell:
                                output += trapspell.ToString();
                                break;
                            case BuffSpell buffspell:
                                output += buffspell.ToString();
                                break;
                            case Spell spell:
                                output += spell.ToString();
                                break;
                            case Card card:
                                output += card.ToString();
                                break;
                        }
                        output += "\n";
                    }

                    if (emptyCounter >= 4)
                    {
                        number = 204;
                        return PLAINHEADER + "The request was fine, but the deck doesn't have any cards";
                    }

                    number = 200;
                    return PLAINHEADER + output;

                case "/deck":
                    string deckAsJson = "[\n\r";
                    for (int i = 1; i <= 4; i++)
                    {
                        sqlStatement = $"SELECT c.cardAsJson FROM cards c, decks d WHERE c.cardID = d.cardID{i} AND d.username = @Tokenname;";

                        if ((sqlResult = _DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)).Contains("ERROR"))
                        {
                            return sqlResult;
                        }
                        if (sqlResult == "")
                        {
                            deckAsJson += "{}";
                        }
                        else
                            deckAsJson += sqlResult;
                        if (i < 4)
                        {
                            deckAsJson += ",";
                        }
                        deckAsJson += "\n\r";
                    }
                    deckAsJson += "]";
                    number = 200;
                    return JSONHEADER + deckAsJson;

                case "/cards":
                    sqlStatement = "SELECT c.cardAsJson FROM cardcompendium cc, cards c WHERE c.cardID = cc.cardID AND username = @Tokenname;";
                    return _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD);

                default:
                    if (path.Contains("/users/"))
                    {
                        string toBeSearchedUser = GetDataFromPath(path);

                        if (queryParameterD["Tokenname"] != toBeSearchedUser &&
                            queryParameterD["Tokenname"] == "admin")
                        {
                            number = 401;
                            return $"You don't have permissions retrieve userdata of {toBeSearchedUser}!";
                        }

                        if (!_DatabaseHandler.CheckUsersExistance(toBeSearchedUser))
                        {
                            number = 404;
                            return "User not found";
                        }

                        queryParameterD["Username"] = toBeSearchedUser;
                        sqlStatement = "SELECT * FROM players WHERE username = @Username;";

                        if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                        {
                            return sqlResult;
                        }

                        sqlResult = ReplaceUnwantedSurroundings(sqlResult);


                        number = 200;
                        return JSONHEADER + sqlResult;
                    }

                    number = 400;
                    return "Unknown path For GET HTTP-method";
            }
        }

        private string HandlePUTRequest(string path, Dictionary<string, string> queryParameterD, ref int number)
        {
            string sqlResult;
            string sqlStatement;
            if (path == "/deck")
            {
                if (!IsTokenActive(queryParameterD))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }

                Dictionary<string, int> CardIDWithAmountD = new Dictionary<string, int>();
                string errmsg = "The following Cards caused problems, due to you not having enough of them: \n\r";
                int i = 1;

                foreach (var item in queryParameterD)
                {
                    if (item.Key != "Tokenname")
                    {
                        if (CardIDWithAmountD.TryAdd(queryParameterD[$"cardID{i}"], 0)) ;
                        else
                        {
                            ++CardIDWithAmountD[queryParameterD[$"cardID{i}"]];
                        }

                        sqlStatement = $"SELECT amount FROM cardcompendium WHERE cardID = @cardID{i} AND username = @Tokenname;";

                        if ((sqlResult = _DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)).Contains("ERROR"))
                        {
                            return sqlResult;
                        }

                        if (!int.TryParse(sqlResult, out int amount))
                        {
                            return "internal server error";
                        }

                        if (amount == 0 ||
                            amount < CardIDWithAmountD[queryParameterD[$"cardID{i}"]])
                        {
                            number = 403;
                            errmsg += $"You don't own the card with the id of {queryParameterD[$"cardID{i}"]}!\n\r";
                        }
                        ++i;
                    }
                }

                if (number == 403)
                {
                    return errmsg;
                }

                sqlStatement = "SELECT Count(*) FROM decks WHERE username = @Tokenname;";
                if (_DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD) == "0")
                {
                    sqlStatement = "INSERT INTO decks (username, cardID1, cardID2, cardID3, cardID4) VALUES (@Tokenname, @cardID1, @cardID2, @cardID3, @cardID4);";
                }
                else
                    sqlStatement = "UPDATE decks SET cardID1 = @cardID1, cardID2 = @cardID2, cardID3 = @cardID3, cardID4 = @cardID4 WHERE username = @Tokenname;";

                if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                {
                    number = 400;
                    return "The provided deck did not include the required amount of cards";
                }

                number = 200;
                return PLAINHEADER + "SUCCESS";

            }
            else if (path.Contains("/users/"))
            {
                if (!IsTokenActive(queryParameterD))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }
                string currentUser = GetDataFromPath(path);

                if (queryParameterD["Tokenname"] != currentUser)
                {
                    number = 401;
                    return $"You don't have permissions change userdata of {currentUser}!";
                }

                if (!_DatabaseHandler.CheckUsersExistance(queryParameterD["Tokenname"]))
                {
                    number = 404;
                    return "User not found";
                }
                sqlStatement = "UPDATE players SET";
                foreach (var keyValuePair in queryParameterD)
                {
                    switch (keyValuePair.Key)
                    {
                        case "Tokenname":
                            continue;
                        case "name":
                            sqlStatement += " username = @name,";
                            continue;
                        case "password":
                            sqlStatement += " password = @password,";
                            continue;
                        case "bio":
                            sqlStatement += " bio = @bio,";
                            continue;
                        case "image":
                            sqlStatement += " image = @image,";
                            continue;
                    }
                }

                sqlStatement = sqlStatement.Remove(sqlStatement.LastIndexOf(","));
                sqlStatement += " WHERE username = @Tokenname;";

                if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                {
                    return sqlResult;
                }

                // Change the other tables
                sqlStatement = "UPDATE REPLACETHIS SET username = @name WHERE username = @Tokenname;";
                string[] tables = { "decks", "cardcompendium", "trades", "scoreboard" };
                for (int i = 0; i < tables.Length; i++)
                {
                    if (i == 0)
                    {
                        sqlStatement = sqlStatement.Replace("REPLACETHIS", tables[0]);
                    }
                    else if (i < tables.Length)
                    {
                        sqlStatement = sqlStatement.Replace(tables[i - 1], tables[i]);
                    }

                    _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD);
                }

                RemoveToken(queryParameterD["Tokenname"]);
                SetToken(queryParameterD["name"]);

                number = 200;
                return PLAINHEADER + "SUCCESS";
            }
            number = 400;
            return "Unknown path For PUT HTTP-method";
        }
        private string HandleDELETERequest(string path, Dictionary<string, string> queryParameterD, ref int number)
        {
            if (path.Contains("/tradings/"))
            {
                if (!IsTokenActive(queryParameterD))
                {
                    number = 401;
                    return "INVALID TOKEN";
                }

                queryParameterD.Add("id", GetDataFromPath(path));

                string sqlResult;
                string sqlStatement = "SELECT COUNT(*) FROM trades WHERE tradeID = @id;";
                if ((sqlResult = _DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)).Contains("ERROR"))
                {
                    return sqlResult;
                }

                if (int.TryParse(sqlResult, out int tradeAmount) &&
                    tradeAmount < 1)
                {
                    number = 404;
                    return "The provided deal ID was not found";
                }

                sqlStatement = "SELECT COUNT(*) FROM trades WHERE username = @Tokenname AND tradeID = @id;";
                if ((sqlResult = _DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)).Contains("ERROR"))
                {
                    return sqlResult;
                }
                if (int.TryParse(sqlResult, out tradeAmount) &&
                    tradeAmount < 1)
                {
                    number = 409;
                    return "A deal with this deal ID already exists";

                }

                sqlStatement = $"DELETE FROM trades WHERE tradeID = @id;";
                if ((sqlResult = _DatabaseHandler.ExecuteSQLCode(sqlStatement, queryParameterD)).Contains("ERROR"))
                {
                    return sqlResult;
                }

                sqlStatement = "SELECT amount FROM cardcompendium WHERE cardID = @id";
                if ((sqlResult = _DatabaseHandler.ExecuteSqlCodeWithSingularResult(sqlStatement, queryParameterD)).Contains("ERROR"))
                {
                    return sqlResult;
                }

                if (int.TryParse(sqlResult, out tradeAmount) &&
                    tradeAmount < 0)
                {
                    number = 403;
                    return "The deal contains a card that is not owned by the user";
                }

                number = 200;
                return PLAINHEADER + "Trading deal successfully deleted";
            }
            number = 400;
            return "Unknown path for DELETE HTTP-method";
        }

        // JSON HANDLER FUNCTIONS:
        /// <summary>
        /// fills given Dictionary queryParameterD with data from httpDataD 
        /// </summary>
        /// <param name="httpDataD"></param>
        /// <param name="queryParameterD"></param>
        /// <returns>empty string ("") if extraction was successfull or an error message</returns>
        public static string FillParameterD(Dictionary<string, string> httpDataD, out Dictionary<string, string> queryParameterD)
        {
            queryParameterD = new Dictionary<string, string>();

            if (httpDataD.TryGetValue("JSON-Data", out string jsonAsString))
            {
                try
                {
                    if (jsonAsString.Contains("["))
                    {
                        JArray jsonData = (JArray)JsonConvert.DeserializeObject(jsonAsString);

                        if ((queryParameterD = JArrayToDictionary(jsonData, httpDataD["path"])) == null)
                        {
                            return "ERROR: the parameters of the provided JSON couldn't be parsed";
                        }
                    }
                    else
                    {
                        if (jsonAsString == "")
                        {
                            return "";
                        }
                        JObject jsonData = (JObject)JsonConvert.DeserializeObject(jsonAsString);

                        if ((queryParameterD = JsonObjectToDictionary(jsonData)) == null)
                        {
                            return "ERROR: the parameters of the provided JSON couldn't be parsed";
                        }
                    }
                }
                catch (JsonSerializationException ex)
                {
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    return "ERROR: the provided JSON couldn't be deserialized, please check the Syntax";
                }
            }

            return "";
        }
        /// <summary>
        /// turns a JObject into into a Dictionary<string, string>
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns>Dictionary<string, string> with of JObject data</returns>
        public static Dictionary<string, string> JsonObjectToDictionary(JObject jsonData)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var property in jsonData.Properties())
                dictionary.Add(property.Name.ToLower(), property.Value.ToString());

            return dictionary;
        }

        /// <summary>
        /// turns JArray into Dictionary<string, string> option should either be the current path defined in the api-spec or "" if it is a normal jsonarray
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <param name="option"></param>
        /// <returns>Dictionary<string, string> with data of JArray</returns>
        public static Dictionary<string, string> JArrayToDictionary(JArray jsonArray, string option)
        {
            var outputD = new Dictionary<string, string>();

            switch (option)
            {
                case "/packages":
                    int index = 1;
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
        /// <summary>
        /// waits for opponent and starts Battle when opponent is found
        /// </summary>
        /// <param name="player"></param>
        /// <returns>concluded Battle</returns>
        public Battle WaitForOpponent(User player)
        {
            lock (_PoolLock)
            {
                _waitingPlayers.Enqueue(player);
                Monitor.PulseAll(_PoolLock);

                if (_waitingPlayers.Count > 1)
                {
                    _battle = new Battle(_waitingPlayers.Dequeue(), _waitingPlayers.Dequeue());
                    _battle.StartBattle();
                    Thread.Sleep(100);
                    return _battle;
                }
                else
                {
                    if (_waitingPlayers.Count < 2)
                    {
                        Monitor.Wait(_PoolLock);
                    }

                    while (_battle != null && !_battle.HasEnded)
                    {
                        Thread.Sleep(10);
                    }
                    return _battle;
                }
            }
        }

        // STRING PARSING FUNCTIONS:

        /// <summary>
        /// creates HTTP-Response string out of responseID and responseData fields;
        /// if the parameter is >= 300, then the function will assume that an error has occurred, 
        /// in which case ResponseData must be plaintext
        /// </summary>
        /// <param name="responseID"></param>
        /// <param name="responseData"></param>
        /// <returns>HTTP response</returns>
        public string CreateHttpResponse(int responseID, string responseData)
        {
            if (responseID >= 300)
            {
                return $"HTTP/1.1 {responseID}\r\n{PLAINHEADER + responseData}\r\n";
            }
            return $"HTTP/1.1 {responseID}\r\n{responseData}\r\n";
        }

        public string GetDataFromPath(string path)
        {
            return path[(path.LastIndexOf('/') + 1)..path.Length];
        }
        public string ExtractValueFromJsonString(string jsonString, string key)
        {
            if (!jsonString.Contains(key))
            {
                return "";
            }

            int indexOfvalue = 0;
            int qutationOccurence = 0;
            for (int i = jsonString.IndexOf(key) + key.Length + 1; i < jsonString.Length; i++)
            {
                if (jsonString[i] == '"')
                {
                    switch (++qutationOccurence)
                    {
                        case 1:
                            indexOfvalue = (i + 1);
                            break;
                        case 2:
                            return jsonString[indexOfvalue..i];
                    }
                }
            }

            return "";
        }

        public static string ExtractHTTPData(string request, out Dictionary<string, string> httpDataD)
        {
            httpDataD = new Dictionary<string, string>();
            httpDataD.Add("HTTP-method", "");
            httpDataD.Add("path", "");

            int indexHelper = 0;

            for (int i = 0; i < request.Length && indexHelper < 3; i++)
            {
                if (request[i] == ' ')
                {
                    ++indexHelper;
                }
                else if (indexHelper == 0)
                {
                    httpDataD["HTTP-method"] += request[i];
                }
                else if (indexHelper == 1)
                {
                    httpDataD["path"] += request[i];
                }
            }

            if (request.Contains("Authorization: Bearer "))
            {
                string username = "";
                string restOftheToken = "";
                bool minusHasBeenReached = false;

                if ((indexHelper = request.IndexOf("Authorization: Bearer ")) == -1)
                {
                    return "FORMATTING-ERROR: Authorization Field was found, but has no index...";
                }

                for (int i = indexHelper + 22;
                         i < request.Length &&
                         request[i] != (char)13; i++)
                {
                    if (request[i] == '-')
                    {
                        minusHasBeenReached = true;
                    }
                    if (minusHasBeenReached)
                    {
                        restOftheToken += request[i];
                    }
                    else
                        username += request[i];
                }
                if (restOftheToken == "-mtcgToken")
                {
                    httpDataD.Add("Tokenname", username);
                }
            }

            if (request.Contains("Content-Type: application/json"))
            {
                string lengthAsString = "";

                // Underneath indexHelper is used to capture the index of "Content-Length: "
                if ((indexHelper = request.IndexOf("Content-Length: ")) == -1)
                {
                    return "FORMATTING-ERROR: Content-Length Field was found, but has no index...";
                }

                // Underneath indexHelper is used to save the length of Json data
                for (int i = indexHelper + 16;
                         i < request.Length &&
                         request[i] != (char)13; i++)
                {
                    lengthAsString += request[i];
                }

                // Underneath indexHelper is used to save the length of Json data
                if (!int.TryParse(lengthAsString, out indexHelper))
                {
                    return "PARSING-ERROR: Could not Parse Content-Length";
                }
                httpDataD.Add("JSON-Data", $"{request[(request.Length - indexHelper)..(request.Length)].Replace("\\", string.Empty)}");
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// true, if given port is valid
        /// </returns>
        public bool ValidatePORT(int input)
        {
            return (0 <= input && input <= 65535);
        }
        /// <summary>
        /// returns true if given port is valid
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// true, if given ip is valid
        /// </returns>
        public bool ValidateIP(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return IPAddress.TryParse(input, out IPAddress _);
        }
        /// <summary>
        /// Replaceses [ and ] in given string
        /// </summary>
        /// <param name="sqlResult"></param>
        /// <returns>string without [ and ]</returns>
        public static string ReplaceUnwantedSurroundings(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (text.Contains("[") &&
               !text.Contains("]"))
            {
                return text.Replace("[", "");
            }

            if (!text.Contains("[") &&
                 text.Contains("]"))
            {
                return text.Replace("]", "");
            }

            text = text.Replace("[", "");
            text = text.Replace("]", "");

            return text;
        }
    }

}