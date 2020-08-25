using ChatClasses.Classes;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace Server
{
    class Start
    {
        static void Main(string[] args)
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            int port = 8888;
            TcpListener server = new TcpListener(localAddr, port);
            UserMessage message = new UserMessage { IP = localAddr.ToString(), Message = "Hello world!" };
            BinaryFormatter formatter = new BinaryFormatter();
            server.Start();
            while (true)
            {
                
                TcpClient tcp = server.AcceptTcpClient();
                Console.WriteLine($"Клиент {tcp.Client} connected");
                NetworkStream stream = tcp.GetStream();
                stream.Write(JsonSerializer.SerializeToUtf8Bytes(message));
                tcp.Close();
                stream.Close();
            }
        }
    }
}
