using ChatClasses.Classes;
using ChatClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    class Start
    {
        static void Main(string[] args)
        {
            string ip = "127.0.0.1";
            if (args.Length > 1)
            {
                ip = args[1];
            }
            Server server = new Server(ip, 8888);
            server.StartServerListener();
        }
    }

    class Server
    {
        public IPAddress ServerIP { get; }

        public int ServerPort { get; }

        public int MaxConnections { get; set; }

        private TcpListener ServerListener { get; set; }

        private List<Connection> clients { get; set; } = new List<Connection>();

        public Server(string ip, int port=8888, int maxConnections=0)
        {
            ServerIP = IPAddress.Any;
            Console.WriteLine(ServerIP);
            ServerPort = port;
            ServerListener = new TcpListener(ServerIP, ServerPort);

            if (maxConnections > 0)
            {
                MaxConnections = maxConnections;
            }
        }

        private void SendMessageToAllConnections(UserMessage message, Connection connection)
        {
            foreach (var client in clients)
            {
                if (connection != client)
                {
                    client.SendData(message);
                }
            }
        }

        private void DeleteClosedConnection(Connection closedConnection)
        {
            if (clients.Contains(closedConnection))
            {
                clients.Remove(closedConnection);
            }
        }
        public void StartServerListener() 
        {
            int current = 1;
            ServerListener.Start();
            while (true)
            {
                TcpClient newClient = ServerListener.AcceptTcpClient();
                Connection newConnection = new Connection(newClient, current++);
                newConnection.ConnectionDisconnected += DeleteClosedConnection;
                newConnection.MessageRecievedEvent += SendMessageToAllConnections;
                clients.Add(newConnection);
                Console.WriteLine("New client connected!");
            }
        }
    }
    class Connection
    {
        public delegate void ConnectionDisconnectedHandler(Connection closedConnection);

        public delegate void MessageRecievedHandler(UserMessage message, Connection connection);

        public event MessageRecievedHandler MessageRecievedEvent;

        public event ConnectionDisconnectedHandler ConnectionDisconnected;
        private TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }

        public int ConnectionNumber;
        public Connection(TcpClient client, int num)
        {
            ConnectionNumber = num;
            Client = client;
            Stream = client.GetStream();
            Task dataTask = new Task(new Action(WaitForData));
            dataTask.Start();
        }

        public void SendData(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            Stream.Write(data, 0, data.Length);
        }

        public void SendData(UserMessage message)
        {
            Stream.Write(JsonSerializer.SerializeToUtf8Bytes(message));
        }

        private void WaitForData()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[10000]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    UserMessage recievedObject;
                    int bytes = 0;
                    do
                    {
                        bytes = Stream.Read(data, 0, data.Length);
                        recievedObject = (UserMessage)JsonSerializer.Deserialize(Encoding.UTF8.GetString(data, 0, bytes), typeof(UserMessage)); 
                        builder.Append(recievedObject.ToString());
                    }
                    while (Stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);//вывод сообщения
                    MessageRecievedEvent?.Invoke(recievedObject, this);
                }
                catch (JsonException)
                {
                    Console.WriteLine("Incorect type server");
                }
                catch
                {
                    Console.WriteLine($"Подключение {ConnectionNumber.ToString()} прервано!"); //соединение было прервано
                    Disconnect();
                    break;
                }
            }
        }
        private void Disconnect()
        {
            if (Client != null)
            {
                Client.Close();
            }
            if (Stream != null)
            {
                Stream.Close();
            }
            ConnectionDisconnected?.Invoke(this);
        }
    }
}
