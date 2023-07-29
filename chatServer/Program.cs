
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace chatServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Console.ReadLine();
            server.Stop();
        }
    }

    public class Server
    {
        private TcpListener listener;
        private ConcurrentBag<TcpClient> clients;
        private ConcurrentDictionary<TcpClient, DateTime> clientLastHeartbeat;
        private Timer heartbeatTimer;
        private bool isRunning = true;
        private SemaphoreSlim heartbeatSemaphore = new SemaphoreSlim(1, 1);

        public Server()
        {
            this.clients = new ConcurrentBag<TcpClient>();
            this.clientLastHeartbeat = new ConcurrentDictionary<TcpClient, DateTime>();
            this.listener = new TcpListener(IPAddress.Any, 8000);
            // Send heartbeat every 30 seconds and check for replies
            this.heartbeatTimer = new Timer(async state => await SafeSendHeartbeatAsync(), null, 0, 10000);
            this.StartListening();
        }

        private async Task SafeSendHeartbeatAsync()
        {
            if (await heartbeatSemaphore.WaitAsync(0))  // Try to acquire the semaphore without waiting
            {
                try
                {
                    await SendHeartbeatAsync();
                  
                    await CheckHeartbeats();  // Check for heartbeat responses after the delay
                }
                finally
                {
                    heartbeatSemaphore.Release();
                }
            }
            else
            {
                Console.WriteLine("[ WARNING ] Previous heartbeat still in progress.");
            }
        }

        private async Task SendHeartbeatAsync()
        {
            byte[] heartbeat = Encoding.ASCII.GetBytes("heartbeat");
            List<Task> tasks = new List<Task>();

            foreach (var client in clients)
            {
                tasks.Add(SendHeartbeatToClientAsync(client, heartbeat));
            }

            await Task.WhenAll(tasks);
        }

        private async Task SendHeartbeatToClientAsync(TcpClient client, byte[] heartbeat)
        {
            try
            {
                NetworkStream clientStream = client.GetStream();
                await clientStream.WriteAsync(heartbeat, 0, heartbeat.Length);
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine($"[ HEARTBEAT SENT | {clientIP} ]");
            }
            catch
            {
                HandleClientDisconnection(client);
            }
        }

        private async Task CheckHeartbeats()
        {
            DateTime now = DateTime.Now;

            var currentClients = clients.ToArray(); // Take a snapshot
            foreach (var client in currentClients)
            {
                if (!clientLastHeartbeat.TryGetValue(client, out DateTime lastHeartbeat))
                {
                    Console.WriteLine($"[ DEBUG | {((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()} ] Client does not have a recorded last heartbeat.");
                    HandleClientDisconnection(client);
                    continue;
                }

                double secondsSinceLastHeartbeat = (now - lastHeartbeat).TotalSeconds;
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine($"[ DEBUG | {clientIP} ] Seconds since last heartbeat: {secondsSinceLastHeartbeat}");

                if (secondsSinceLastHeartbeat > 11)
                {
                    Console.WriteLine($"[ DEBUG | {clientIP} ] Disconnecting client due to missed heartbeat.");
                    HandleClientDisconnection(client);
                }
            }
        }

        private void HandleClientDisconnection(TcpClient client)
        {
            if (client == null) return;

            // Attempt to remove the client from the list
            bool clientRemoved = false;
            while (!clientRemoved)
            {
                if (clients.TryTake(out TcpClient removedClient))
                {
                    if (client == removedClient)
                    {
                        clientRemoved = true;
                    }
                    else
                    {
                        // This wasn't the client we intended to remove, so add it back
                        clients.Add(removedClient);
                    }
                }
                else
                {
                    // Couldn't remove any client, break out of the loop
                    break;
                }
            }

            clientLastHeartbeat.TryRemove(client, out _);

            if (client.Client != null && client.Client.RemoteEndPoint != null)
            {
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ DISCONNECT | {clientIP} ] Client disconnected");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ DISCONNECT ] Client disconnected");
                Console.ResetColor();
            }
        }

        private void StartListening()
        {
            this.listener.Start();
            Console.WriteLine($"Chat Server started on {createInvite(IPAddress.Any.ToString())} 8000");
            while (isRunning)
            {
                try
                {
                    TcpClient client = this.listener.AcceptTcpClient();
                    this.clients.Add(client);
                    this.clientLastHeartbeat[client] = DateTime.Now; // Initialize the last heartbeat time
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
                catch (SocketException)
                {
                    // Handle potential errors gracefully
                    break;
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            this.listener.Stop();
            this.heartbeatTimer.Dispose();
            foreach (var client in clients)
            {
                client.Close();
            }
        }

        private string createInvite(string ip)
        {
            byte[] ipBytes = Encoding.UTF8.GetBytes("192.168.3.68");
            return Convert.ToBase64String(ipBytes);
        }

        private void HandleClient(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            string clientIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[ CONNECT | {0} ] Client connected from {1}", clientIP, DateTime.Now);
            Console.ResetColor();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    // Read incoming message from client
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch (Exception ex)
                {
                    // Handle exception
                    break;
                }

                if (bytesRead == 0)
                {
                    // Client disconnected
                    break;
                }

                // Convert incoming message to string
                string data = Encoding.ASCII.GetString(message, 0, bytesRead);

                if (data == "HEARTBEAT-REPLY")
                {
                    clientLastHeartbeat[tcpClient] = DateTime.Now; // Update the last heartbeat time
                    Console.WriteLine($"[ HEARTBEAT REPLY RECEIVED | {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()} ]");
                    continue;
                }

                string logMessage = String.Format("[ MESSAGE | {0} ] Message from client: {1}", clientIP, data);

                // by any means this should be shown for security and privacy reasons.
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(logMessage);
                Console.ResetColor();

                // Broadcast message to all connected clients
                byte[] broadcastMessage = Encoding.ASCII.GetBytes(logMessage);
                foreach (TcpClient connectedClient in clients)
                {
                    if (connectedClient == tcpClient)
                    {
                        continue;
                    }

                    NetworkStream connectedClientStream = connectedClient.GetStream();
                    connectedClientStream.Write(broadcastMessage, 0, broadcastMessage.Length);
                }
            }

            // Handle client disconnection
            HandleClientDisconnection(tcpClient);

        }
    }
}