using ChatClasses.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
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
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;

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
            
            Client.ConnectionLostEvent += ChangeConnectionStatusImage_Disconnect;
            Client.ConnectionLostEvent += Client.TryToConnect;
            Client.ConnectionSuccessEvent += ChangeConnectionStatusImage_Connect;
            
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

        private void ChangeConnectionStatusImage_Disconnect()
        {
            Dispatcher?.Invoke(new Action(() => ConnectionStatusImage.Source = new BitmapImage(new Uri("C:/Users/User/source/repos/ChatCSWPF/ClientAppWpf/Icons/disconnected.png", UriKind.RelativeOrAbsolute))));
        }

        private void ChangeConnectionStatusImage_Connect()
        {
            Dispatcher?.Invoke(new Action(() => ConnectionStatusImage.Source = new BitmapImage(new Uri("C:/Users/User/source/repos/ChatCSWPF/ClientAppWpf/Icons/connected.png", UriKind.RelativeOrAbsolute))));
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string text = MessageTextBox.Text;
            if (text.Length > 0) {
                UserMessage newMessage = new UserMessage { Message = text};
                Messages.Add(newMessage);
                Client.SendMessage(newMessage);
                MessageTextBox.Clear();
            }
        }

        private void NewMessage_Recieved(UserMessage message)
        {
            Dispatcher?.Invoke(new Action(() => Messages.Add(message)));
        }
    }

    
}
