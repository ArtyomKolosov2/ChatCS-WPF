using ChatClasses.Classes;
using ChatClasses.Interfaces;
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
            UserClient client = new UserClient("127.0.0.1", 8888);
            client.TryToConnect();
            while (true)
            {
                Console.WriteLine("Input message:");
                UserMessage message = new UserMessage { Message = Console.ReadLine() };
                client.SendMessage(message);
            }
        }
    }

    
}
