using ChatClasses.Classes;
using System;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect("127.0.0.1", 8888);
                StringBuilder response = new StringBuilder();
                byte[] data = new byte[1024];
                NetworkStream stream = client.GetStream();
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    UserMessage message = (UserMessage)JsonSerializer.Deserialize(Encoding.UTF8.GetString(data, 0, bytes), typeof(UserMessage));
                    Console.WriteLine(message.Message);
                }
                while (stream.DataAvailable);
                client.Close();
                stream.Close();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
