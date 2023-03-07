using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace chatServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();

            Console.ReadLine();
        }
    }

    public class Server
    {
        private TcpListener listener;
        private Thread listenerThread;
        private List<TcpClient> clients;
        private Timer heartbeatTimer;

        public Server()
        {
            this.clients = new List<TcpClient>();
            this.listener = new TcpListener(IPAddress.Any, 8000);
            this.listenerThread = new Thread(new ThreadStart(ListenForClients));
            this.listenerThread.Start();

            // To start the timer with the asynchronous method:
            this.heartbeatTimer = new Timer(async state => await SendHeartbeatAsync(state), null, 0, 30000);
        }

        private async Task SendHeartbeatAsync(object state)
        {
            if (this.clients.Count > 0)
            {
                for (int i = this.clients.Count - 1; i >= 0; i--)
                {
                    TcpClient client = this.clients[i];
                    NetworkStream clientStream = client.GetStream();

                    try
                    {
                        // Send heartbeat message
                        byte[] heartbeat = Encoding.ASCII.GetBytes("heartbeat");
                        clientStream.Write(heartbeat, 0, heartbeat.Length);

                        Console.WriteLine("[ HEARTBEAT ] sent to client {0}", i);

                        // Receive heartbeat reply
                        byte[] reply = new byte[4096];
                        int bytesRead = await clientStream.ReadAsync(reply, 0, 4096);
                        string data = Encoding.ASCII.GetString(reply, 0, bytesRead);

                        if (data.Length == 0)
                        {
                            // Remove unreachable client
                            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[ DISCONNECT | {0} ] Client unreachable", clientIP);
                            Console.ResetColor();
                            this.clients.RemoveAt(i);
                        }

                        if (data == "HEARTBEAT-REPLY")
                        {
                            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("[ VERIFIED | {0} ] Client Replied!", clientIP);
                            Console.ResetColor();
                        }

                    }
                    catch (ObjectDisposedException)
                    {
                        // Remove disconnected client
                        string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ DISCONNECT | {0} ] Client disconnected", clientIP);
                        Console.ResetColor();
                        this.clients.RemoveAt(i);
                    }
                    catch (IOException)
                    {
                        // Remove disconnected client
                        string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ DISCONNECT | {0} ] Client disconnected", clientIP);
                        Console.ResetColor();
                        this.clients.RemoveAt(i);
                    }
                    catch (ArgumentOutOfRangeException)
                    {

                    }
                }
            }
        }

        private void ListenForClients()
        {
            this.listener.Start();
            Console.WriteLine("Chat Server made by s1");
            Console.WriteLine();
            Console.WriteLine("Chat server started on port 8000");

            while (true)
            {
                TcpClient client = null;

                try
                {
                    client = this.listener.AcceptTcpClient();
                    this.clients.Add(client);

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.Start(client);
                }
                catch (Exception ex)
                {
                    // Handle exception
                    Console.WriteLine("Error accepting client: {0}", ex.Message);
                    if (client != null) clients.Remove(client);
                }
            }
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

                string logMessage = String.Format("[ MESSAGE | {0} ] Message from client: {1}", clientIP, data);
                
                // Skip heartbeat messages
                if (data == "HEARTBEAT-REPLY")
                {
                    continue;
                }

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

        }
    }
}