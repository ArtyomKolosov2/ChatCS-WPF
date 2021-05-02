using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Config;
using Common.Models;

namespace Server.Models
{
    internal class Connection
    {
        public delegate void ConnectionDisconnectedHandler(Connection closedConnection);

        public delegate void MessageReceivedHandler(UserMessage message, Connection connection);

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

        public void SendData(UserMessage message)
        {
            Stream.Write(GlobalConfig.Encoding.GetBytes(JsonSerializer.Serialize(message)));
        }

        private void WaitForData()
        {
            while (true)
            {
                try
                {
                    var data = new byte[GlobalConfig.Size]; 
                    var builder = new StringBuilder();
                    UserMessage receivedObject;

                    do
                    {
                        var bytes = Stream.Read(data, 0, data.Length);
                        receivedObject = JsonSerializer.Deserialize<UserMessage>
                        (
                            GlobalConfig.Encoding.GetString(data, 0, bytes)
                        );

                        builder.Append(receivedObject);
                    }
                    while (Stream.DataAvailable);

                    var message = builder.ToString();
                    Console.WriteLine(message);
                    MessageReceivedEvent?.Invoke(receivedObject, this);
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
