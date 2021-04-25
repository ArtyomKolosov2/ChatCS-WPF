using Common.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media.Imaging;
using Common.GlobalConfig;

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
            
            Client = new UserClient(GlobalConfig.DefaultServerGateway, GlobalConfig.Port);
            
            Client.ConnectionLostEvent += ChangeConnectionStatusImage_Disconnect;
            Client.ConnectionLostEvent += Client.TryToConnect;
            Client.ConnectionSuccessEvent += ChangeConnectionStatusImage_Connect;
            
            await Client.TryToConnectAsync();
            Client.MessageReceivedEvent += NewMessage_Recieved;
            
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
            Dispatcher?.Invoke(() => ConnectionStatusImage.Source = new BitmapImage(new Uri("Icons/disconnected.png", UriKind.Relative)));
        }

        private void ChangeConnectionStatusImage_Connect()
        {
            Dispatcher?.Invoke(() => ConnectionStatusImage.Source = new BitmapImage(new Uri("Icons/connected.png", UriKind.Relative)));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var text = MessageTextBox.Text;

            if (text.Length > 0) 
            {

                var newMessage = new UserMessage
                {
                    Message = text
                };

                Messages.Add(newMessage);
                Client.SendMessage(newMessage);

                MessageTextBox.Clear();
            }
        }

        private void NewMessage_Recieved(UserMessage message)
        {
            Dispatcher?.Invoke(() => Messages.Add(message));
        }

    }
}
