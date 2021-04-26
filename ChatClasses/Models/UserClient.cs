using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Common.Models
{
    public class UserClient 
    {
        public delegate void ConnectionLostHandler();
        public delegate void ConnectionSuccessHandler();
        public delegate void MessageReceivedHandler(UserMessage message);

        public event ConnectionLostHandler ConnectionLostEvent;
        public event ConnectionSuccessHandler ConnectionSuccessEvent;
        public event MessageReceivedHandler MessageReceivedEvent;

        public IPAddress ServerIP { get; set; }
        public TcpClient TcpClient { get; private set; }
        public NetworkStream Stream { get; private set; }
        public int ServerPort { get; set; }
        private Task ReceiverTask { get; set; }

        public UserClient(string ip, int port)
        {
            ServerIP = IPAddress.Parse(ip);
            ServerPort = port;
        }

        public void SendMessage(UserMessage message)
        {
            if (TcpClient != null && TcpClient.Connected)
            {
                Stream.Write(JsonSerializer.SerializeToUtf8Bytes(message));
            }
        }

        private static async void RunEventDelegatesAsync(Delegate[] delegates)
        {
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
                    var builder = new StringBuilder();
                    var data = new byte[GlobalConfig.GlobalConfig.Size];
                    UserMessage receivedObject;

                    do
                    {

                        var bytes = Stream.Read(data, 0, data.Length);
                        receivedObject = JsonSerializer.Deserialize<UserMessage>(Encoding.UTF8.GetString(data, 0, bytes));

                        builder.Append(receivedObject);
                    }
                    while (Stream.DataAvailable);

                    MessageReceivedEvent?.Invoke(receivedObject);

                    var message = builder.ToString();
                    Console.WriteLine(message);
                }

                catch (JsonException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;

                }

                catch (Exception ex) when (ex is SocketException || ex is IOException)
                {
                    Console.WriteLine($"Error: {ex.Message} = {ex.GetType()} Подключение прервано!");
                    RunEventDelegatesAsync(ConnectionLostEvent?.GetInvocationList());
                    break;
                }
            }
        }

        public void Disconnect()
        {
            if (TcpClient != null)
            {
                TcpClient.Close();
            }
            if (Stream != null)
            {
                Stream.Close();
            }
        }

        public async Task TryToConnectAsync()
        {
            await Task.Run(() => TryToConnect());
        }

        public void TryToConnect()
        {
            while (!Connect())
            {
                Console.WriteLine("Connection lost... Retrying");
                Thread.Sleep(5000);
            }
        }

        private bool Connect()
        {
            var connectResult = false;
            try
            {
                TcpClient = new TcpClient();
                TcpClient.Connect(ServerIP, ServerPort);
                Stream = TcpClient.GetStream();
                ReceiverTask = new Task(ReciveMessages);
                ReceiverTask.Start();

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
