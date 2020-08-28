using ChatClasses.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientAppWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UserClient Client { get; private set; }
        public ObservableCollection<UserMessage> Messages { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Messages = new ObservableCollection<UserMessage>();
            MessagesListView.ItemsSource = Messages;
            
            Client = new UserClient("127.0.0.1", 8888);
            await Task.Run(() => Client.TryToConnect());
            Client.MessageRecivedEvent += NewMessage_Recieved;
            ((INotifyCollectionChanged)MessagesListView.ItemsSource).CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    MessagesListView.ScrollIntoView(MessagesListView.Items[MessagesListView.Items.Count - 1]);
                }
            };

        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            UserMessage newMessage = new UserMessage { Message = MessageTextBox.Text };
            Messages.Add(newMessage);
            Client.SendMessage(newMessage);
            MessageTextBox.Clear();
        }

        private void NewMessage_Recieved(UserMessage message)
        {
            Dispatcher?.Invoke(new Action(() => Messages.Add(message)));
        }
    }

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
            ConnectionLostEvent += TryToConnect;
        }
        ~UserClient(){ Disconnect(); }
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
                Stream.Write(JsonSerializer.SerializeToUtf8Bytes(message));
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
                        recievedObject = (UserMessage)JsonSerializer.Deserialize(Encoding.UTF8.GetString(data, 0, bytes), typeof(UserMessage));
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message} = {ex.GetType()} Подключение прервано!");
                    ConnectionLostEvent?.Invoke();
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
