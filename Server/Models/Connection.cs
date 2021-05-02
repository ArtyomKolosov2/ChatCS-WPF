using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Config;
using Common.Models;
using Common.Utils;


namespace Server.Models
{
    internal class Connection
    {
        public delegate void ConnectionDisconnectedHandler(Connection closedConnection);

        public delegate void MessageReceivedHandler(ReadOnlySpan<byte> message, Connection connection);

        public event MessageReceivedHandler MessageReceivedEvent;

        public event ConnectionDisconnectedHandler ConnectionDisconnected;
        private TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }

        public Guid Id { get; private set; }
        public Connection(TcpClient client)
        {
            Id = Guid.NewGuid();
            
            Client = client;
            Stream = client.GetStream();

            var dataTask = new Task(WaitForData);

            dataTask.Start();
        }

        public void SendData(ReadOnlySpan<byte> message, int bytes = GlobalConfig.Size)
        {
            Stream.Write(message);
        }

        private void WaitForData()
        {
            while (true)
            {
                try
                {
                    var data = new byte[GlobalConfig.Size]; 
                    var builder = new StringBuilder();
                    ReadOnlySpan<byte> realData;

                    do
                    {
                        var bytes = Stream.Read(data, 0, data.Length);
                        realData = new ReadOnlySpan<byte>(data, 0, bytes);

                        builder.Append(bytes);
                    }
                    while (Stream.DataAvailable);

                    var message = builder.ToString();
                    Console.WriteLine(message);
                    MessageReceivedEvent?.Invoke(realData, this);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"{ex.Message} {ex.GetType()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Подключение {Id} прервано!");
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
