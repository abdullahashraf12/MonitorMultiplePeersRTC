using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MonitorMultiplePeersRTC.WebSocketRTC
{
    public static class WebSocketHandler
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> groupClients = new();

        private static readonly List<string> otherclients;

        private static HttpContext _context;

        private static void AddClientToGroup(string groupno, string socketId, WebSocket socket)
        {
            var group = groupClients.GetOrAdd(groupno, _ => new ConcurrentDictionary<string, WebSocket>());
            group[socketId] = socket;
        }
        
        private static bool IsGroupExists(string groupNo)
        {
            // Check if the group exists in the groupClients dictionary
            return groupClients.ContainsKey(groupNo);
        }

        private static string GetGroupByClientIdPublic(string clientId)
        {
            // Iterate over all groups to check if the clientId exists
            foreach (var group in groupClients)
            {
                // Check if the clientId exists in the current group
                if (group.Value.ContainsKey(clientId))
                {
                    
                    Console.WriteLine($"Found Client With GROUP KEY: {group.Key}");
                    // Return the groupNo (the key of the group)
                    return group.Key;
                   
             
                }
            }

            return "";
        }

        // This method will accept the WebSocket connection and delegate further handling
        public static async Task AcceptAndHandleWebSocketAsync(HttpContext context)
        {
            _context = context;

            var token = _context.Request.Query["Token"].ToString().Replace(" ", "");
            var groupNo = _context.Request.Query["groupNo"].ToString().Replace(" ", "");
            var ClientId = _context.Request.Query["ClientId"].ToString().Replace(" ", "");
            var connectiontype = _context.Request.Query["ScreenShareFrom"].ToString().Replace(" ", "");

            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Group " + groupNo);
                Console.WriteLine("ClientId " + ClientId);
                Console.WriteLine("ClientId By Group " + GetGroupByClientIdPublic(ClientId));
                Console.WriteLine("Does Group Exists " + IsGroupExists(groupNo)+groupNo);

                if (IsGroupExists(groupNo)==true && GetGroupByClientIdPublic(ClientId)=="") { 
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                string socketId = context.Session.GetString("UniqueNumber");

                        //Guid.NewGuid().ToString(); // Use socket ID or any unique identifier for the connection

                //clients[socketId] = socket;
                AddClientToGroup(groupNo, socketId, socket);
                // Add the new peer
                //AddPeer(socketId, socket);
                Console.WriteLine($"A new client connected with socket ID: {socketId} here 22");

                // Notify all other peers to setup the peer connection receiver
                await NotifyPeers(groupNo, socketId);

                // Start listening to WebSocket messages
                await ListenToWebSocketAsync(groupNo, socket, socketId);
                    //clients[socketId] = socket;
                }
                else if (IsGroupExists(groupNo)==false && GetGroupByClientIdPublic(ClientId) != "")
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    string socketId = context.Session.GetString("UniqueNumber");


                    //Guid.NewGuid().ToString(); // Use socket ID or any unique identifier for the connection

                    groupNo = GetGroupByClientIdPublic(ClientId);
                    AddClientToGroup(groupNo, socketId, socket);
                    // Add the new peer
                    //AddPeer(socketId, socket);
                    Console.WriteLine($"A new client connected with socket ID: {socketId} here 33");

                    // Notify all other peers to setup the peer connection receiver
                    await NotifyPeers(groupNo, socketId);

                    // Start listening to WebSocket messages
                    await ListenToWebSocketAsync(groupNo, socket, socketId);
                }
                else if (string.IsNullOrEmpty(groupNo) && IsGroupExists(groupNo) ==true && string.IsNullOrEmpty(ClientId))
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    string socketId = context.Session.GetString("UniqueNumber");
                    groupNo = groupNo;
                    AddClientToGroup(groupNo, socketId, socket);
                    // Add the new peer
                    //AddPeer(socketId, socket);
                    Console.WriteLine($"A new client connected with socket ID: {socketId} here 11");

                    // Notify all other peers to setup the peer connection receiver
                    await NotifyPeers(groupNo, socketId);

                    // Start listening to WebSocket messages
                    await ListenToWebSocketAsync(groupNo, socket, socketId);
                }
                
                
                else
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    string socketId = context.Session.GetString("UniqueNumber");
                    groupNo = GenerateRandomNumberString(32);
                    //var message = new
                    //{
                    //    type = "GroupNumber",
                    //    groupNo = groupNo,
                    //    socket_Id = socketId
                    //};

                    // Send the response message to the target WebSocket
                    //await SendMessage(socket, JsonConvert.SerializeObject(message));
                    AddClientToGroup(groupNo, socketId, socket);
                    // Add the new peer
                    //AddPeer(socketId, socket);
                    Console.WriteLine($"A new client connected with socket ID: {socketId} here 15");

                    // Notify all other peers to setup the peer connection receiver
                    await NotifyPeers(groupNo, socketId);

                    // Start listening to WebSocket messages
                    await ListenToWebSocketAsync(groupNo, socket, socketId);
                }
            }
            else
            {
                //Console.WriteLine("HEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEER");
            }

        }

        public static string GenerateRandomNumberString(int length)
        {
            var random = new Random();
            const string digits = "0123456789";
            var stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = digits[random.Next(digits.Length)];
            }
            return new string(stringChars);
        }



        // Notify all other peers about a new connection
        public static async Task NotifyPeers(string groupno, string socketId)
        {
            foreach (var group in groupClients)
            {
                string group_no = group.Key; // The group number (key of the outer dictionary)

                // Check if the group number is "1"
                if (group_no == groupno)
                {
                    var groupSockets = group.Value; // The inner dictionary (key = socketId, value = WebSocket)

                    // Loop through each socket in the group's inner dictionary
                    foreach (var socket in groupSockets)
                    {
                        string socket_Id = socket.Key; // The socket ID (key of the inner dictionary)
                        WebSocket webSocket = socket.Value; // The WebSocket object (value of the inner dictionary)

                        // Now you can work with groupno, socketId, and webSocket
                        if (socket_Id != socketId)
                        {
                            var message = new
                            {
                                type = "initReceive",
                                socketId = socketId
                            };
                            Console.WriteLine($"Group: {groupno}, SocketId: {socketId} is notified");

                            // Send the response message to the target WebSocket
                            await SendMessage(webSocket, JsonConvert.SerializeObject(message));
                        }
                        else
                        {
                            Console.WriteLine($"Group: {groupno}, SocketId: {socketId} is not notified");

                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Group {groupno} not found or has no connected clients.");
                }
            }
        }


       


        // Listen to WebSocket messages
        private static async Task ListenToWebSocketAsync(string groupno, WebSocket socket, string socketId)
        {
            byte[] buffer = new byte[1024 * 1024 * 1024];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await RemoveClientFromGroup(groupno, socketId);
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                    }
                    else
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await HandleMessageAsync(groupno, socketId, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in WebSocket communication: {ex.Message}");
                await RemoveClientFromGroup(groupno, socketId);
            }
        }


        private static async Task RemoveClientFromGroup(string groupno, string socketId)
        {
            if (groupClients.TryGetValue(groupno, out var group))
            {
                if (group.TryRemove(socketId, out var socket))
                {
                    Console.WriteLine($"Client {socketId} removed from group {groupno}");

                    // Notify remaining clients in the group
                    var message = new { type = "clientDisconnected", socketId };

                    foreach (var clientSocket in group.Values)
                    {
                        await SendMessage(clientSocket, JsonConvert.SerializeObject(message));
                    }
                }
            }
        }





        private static async Task HandleMessageAsync(string groupno, string senderSocketId, string message)
        {
            // Check if the message is not empty
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine(message);
                var data = JsonConvert.DeserializeObject<dynamic>(message);
                string typeString = data.ToString().Replace(",, \\", "").Replace(",,", " ,");  // Trim to remove any unwanted spaces

                data = JsonConvert.DeserializeObject<dynamic>(typeString);
                // Check the type of message received
                // Check for recognized message types first
                if (data.type == "signal")
                {
                    Console.WriteLine("Received signal data: " + data);

                    // Extract the target socket ID and signal information from the message
                    string targetSocketId = data.socket_id;
                    string signal = data.signal.ToString();
                    WebSocket ws=null;
                    if (groupClients.TryGetValue(groupno, out var targetSocket)){
                        targetSocket.TryGetValue(targetSocketId, out ws);
                        var response = new
                        {
                            type = "signal",      // Type of message being sent
                            socketId = senderSocketId,  // The sender's socket ID
                            signal = signal      // The signal data
                        };
                        Console.WriteLine("this signal should be out =>" + JsonConvert.SerializeObject(response).ToString());
                        // Send the response message to the target WebSocket
                        await SendMessage(ws, JsonConvert.SerializeObject(response));
                        

                    }
                    
                }
                else if (data.type == "initSend")
                {
                    Console.WriteLine("Received initSend data: " + data);

                    // Extract the socket ID of the initiator
                    string initSocketId = data.socket_id;
                    WebSocket ws = null;
                    if (groupClients.TryGetValue(groupno, out var targetSocket))
                    {
                        targetSocket.TryGetValue(initSocketId, out ws);
                       
                        var response = new
                        {
                            type = "initSend",  // Type of message being sent
                            socketId = senderSocketId  // The sender's socket ID
                        };

                        // Send the response message to the target WebSocket
                        await SendMessage(ws, JsonConvert.SerializeObject(response));
                                //}
                            //}
                        //}
                    }
                }
                else if (data.type == "removePeer")
                {
                    Console.WriteLine("Received removePeer data: " + data);

                    // Extract the peer ID to remove
                    string peerId = data.peerId;
                    WebSocket ws = null;
                    if (groupClients.TryGetValue(groupno, out var targetSocket))
                    {
                        targetSocket.TryGetValue(peerId, out ws);
                        var response = new
                        {
                            type = "removePeer",  // Type of message being sent
                            socketId = senderSocketId  // The sender's socket ID
                        };
                        // Send the response message to the target WebSocket
                        await SendMessage(ws, JsonConvert.SerializeObject(response));
                       
                    }
                }
                else if (data.type == "initReceive")
                {
                    Console.WriteLine("Received initReceive data: " + data);

                    // Extract the new peer socket ID
                    string newPeerSocketId = data.newPeerSocketId;

                    foreach (var group in groupClients)
                    {
                        string group_no = group.Key; // The group number (key of the outer dictionary)

                        // Check if the group number is "1"
                        if (group_no == groupno)
                        {
                            var groupSockets = group.Value; // The inner dictionary (key = socketId, value = WebSocket)

                            // Loop through each socket in the group's inner dictionary
                            foreach (var socket in groupSockets)
                            {
                                string socketId = socket.Key; // The socket ID (key of the inner dictionary)
                                WebSocket webSocket = socket.Value; // The WebSocket object (value of the inner dictionary)
                                if (socketId == newPeerSocketId)
                                {
                                    // Now you can work with groupno, socketId, and webSocket
                                    Console.WriteLine($"Group: {groupno}, SocketId: {socketId}");
                                    var response = new
                                    {
                                        type = "initSend",  // Type of message being sent
                                        socketId = senderSocketId  // The sender's socket ID
                                    };
                                    // Send the response message to the target WebSocket
                                    await SendMessage(webSocket, JsonConvert.SerializeObject(response));
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("I am Here");
                    // This part will only execute if the data.type is not one of the recognized types
                    // So, the "Unrecognised message" lines will be skipped
                    if (data.type != "signal" && data.type != "initSend" && data.type != "removePeer" && data.type != "initReceive")
                    {
                        Console.WriteLine("Unrecognised message: " + data);
                        Console.WriteLine("Unrecognised message: " + data.type);
                        Console.WriteLine("Unrecognised message: " + data.signal);
                    }
                }

            }
            else
            {
                // If the message type is not recognized, print the message data
                Console.WriteLine("Received empty message: " + message);
            }

        }

        // Send a message to the WebSocket
        private static async Task SendMessage(WebSocket socket, string message)
        {

            if (socket.State == WebSocketState.Open)
            {
                Console.WriteLine("message type is " + message.GetType());
                Console.WriteLine("message is " + message.ToString());

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        // Handle case when WebSocket is not open
                        Console.WriteLine("WebSocket is not open.");
                    }
                }
                catch (WebSocketException ex)
                {
                    // Handle WebSocket specific exceptions
                    Console.WriteLine($"WebSocket exception: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle any other exceptions
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
            //}
        }


        private static async Task BroadcastToGroup(string groupno, string senderSocketId, dynamic content)
        {
            if (groupClients.TryGetValue(groupno, out var group))
            {
                foreach (var (socketId, socket) in group)
                {
                    if (socketId != senderSocketId)
                    {

                        await SendMessage(socket, content);
                    }
                }
            }
        }
    }
}
