using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClasses.Classes
{
    public class UserClient
    {
        public delegate void ConnectionLostHandler();
        public delegate void ConnectionSuccessHandler();
        public delegate void MessageRecivedHandler(UserMessage message);

        public event ConnectionLostHandler ConnectionLostEvent;
        public event ConnectionSuccessHandler ConnectionSuccessEvent;
        public event MessageRecivedHandler MessageRecivedEvent;
        public IPAddress ServerIP { get; set; }
        public TcpClient tcpClient { get; private set; }
        public NetworkStream Stream { get; private set; }
        public int ServerPort { get; set; }
        private Task recieverTask { get; set; }
        public UserClient(string ip, int port)
        {
            ServerIP = IPAddress.Parse(ip);
            ServerPort = port;

        }
        ~UserClient() { Disconnect(); }
        public void SendMessage(string message)
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                Stream.Write(Encoding.UTF8.GetBytes(message));
            }
        }
        public void SendMessage(UserMessage message)
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                Stream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            }
        }
        private async void RunEventDelegatesAsync(Delegate[] delegates)
        {
            Delegate[] Delegatelist = ConnectionLostEvent.GetInvocationList();
            foreach (var del in delegates)
            {
                await Task.Run(() => del?.DynamicInvoke());
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
                    UserMessage recievedObject;
                    int bytes = 0;
                    do
                    {
                        bytes = Stream.Read(data, 0, data.Length);
                        recievedObject = (UserMessage)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data, 0, bytes), typeof(UserMessage));
                        builder.Append(recievedObject.ToString());
                    }
                    while (Stream.DataAvailable);
                    MessageRecivedEvent?.Invoke(recievedObject);
                    string message = builder.ToString();
                    Console.WriteLine(message);
                }
                catch (JsonException)
                {
                    Console.WriteLine("Incorrect type");
                    break;

                }
                catch (Exception ex) when (ex is SocketException || ex is IOException)
                {
                    Console.WriteLine($"Error: {ex.Message} = {ex.GetType()} Подключение прервано!");
                    RunEventDelegatesAsync(ConnectionLostEvent.GetInvocationList());
                    break;
                }
            }
        }

        public void Disconnect()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
            if (Stream != null)
            {
                Stream.Close();
            }
        }
        public void TryToConnect()
        {
            while (Connect() == false)
            {
                Console.WriteLine("Connection lost... Retrying");
                Thread.Sleep(5000);
            }
        }
        private bool Connect()
        {
            bool connectResult = false;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ServerIP, ServerPort);
                Stream = tcpClient.GetStream();
                recieverTask = new Task(new Action(ReciveMessages));
                recieverTask.Start();
                connectResult = true;
                ConnectionSuccessEvent?.Invoke();
            }
            catch (SocketException)
            {
                connectResult = false;
            }
            return connectResult;
        }
    }
}
