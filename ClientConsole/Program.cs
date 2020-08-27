using ChatClasses.Classes;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client("127.0.0.1", 8888);
            client.Connect();
            while (true)
            {
                Console.WriteLine("Input message:");
                client.SendMessage(Console.ReadLine());
            }
        }
    }

    class Client
    {
        public delegate void ConnectionLostHandler();
        public delegate void ConnectionSuccessHandler();
        public event ConnectionLostHandler ConnectionLostEvent;
        public event ConnectionSuccessHandler ConnectionSuccessEvent;
        public IPAddress ServerIP { get; set; }
        public TcpClient tcpClient { get; private set; }
        public int ServerPort { get; set; }

        private Task recieverTask { get; set; }
        public Client(string ip, int port)
        {
            ServerIP = IPAddress.Parse(ip);
            ServerPort = port;
            ConnectionLostEvent += Reconnect;
        }

        public void SendMessage(string message)
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                tcpClient.GetStream().Write(Encoding.UTF8.GetBytes(message));
            }
        }

        private void ReciveMessages()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[1024]; 
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = tcpClient.GetStream().Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (tcpClient.GetStream().DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message} Подключение прервано!");
                    ConnectionLostEvent?.Invoke();
                    Disconnect();
                    break;
                }
               /* catch 
                {
                    Console.WriteLine("Unexpected error!");
                    break;
                }*/
            }
        }

        public void Disconnect()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
        }
        private void Reconnect()
        {
            while (Connect() == false)
            {
                Console.WriteLine("Connection lost... Retrying");
                Thread.Sleep(1000);
            }
        }
        public bool Connect()
        {
            bool connectResult = false;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ServerIP, ServerPort);
                recieverTask = new Task(new Action(ReciveMessages));
                recieverTask.Start();
                ConnectionSuccessEvent?.Invoke();
                connectResult = true;
            }
            catch
            {
                connectResult = false;
            }
            return connectResult;
        }
    }
}
