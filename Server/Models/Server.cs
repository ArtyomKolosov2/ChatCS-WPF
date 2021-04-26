using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common.Models;

namespace Server.Models
{
    public class Server
    {
        public IPAddress ServerIp { get; }

        public int ServerPort { get; }

        private TcpListener ServerListener { get; set; }

        private Dictionary<Guid, Connection> Clients { get; } = new ();

        public Server(string ip, int port = 8888)
        {
            ServerIp = IPAddress.Parse(ip);
            Console.WriteLine(ServerIp);
            ServerPort = port;

            ServerListener = new TcpListener(ServerIp, ServerPort);
        }

        private void SendMessageToAllConnections(UserMessage message, Connection connection)
        {
            foreach (var client in Clients.Values)
            {
                if (connection.Id != client.Id)
                {
                    client.SendData(message);
                }
            }
        }

        private void DeleteClosedConnection(Connection closedConnection)
        {
            if (Clients.ContainsKey(closedConnection.Id))
            {
                Clients.Remove(closedConnection.Id);
            }
        }

        public void StartServerListener()
        {
            ServerListener.Start();
            while (true)
            {
                var newClient = ServerListener.AcceptTcpClient();
                var newConnection = new Connection(newClient);
                newConnection.ConnectionDisconnected += DeleteClosedConnection;
                newConnection.MessageReceivedEvent += SendMessageToAllConnections;

                Clients[newConnection.Id] = newConnection;

                Console.WriteLine("New client connected!");
            }
        }
    }
}