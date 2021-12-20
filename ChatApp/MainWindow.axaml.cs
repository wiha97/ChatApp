using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ChatApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            View.SetWin(this);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Connect(object sender, RoutedEventArgs e)
        {
            View.Connect();
        }

        public void Host(object sender, RoutedEventArgs e)
        {
            View.Host();
        }

        public void Send(object sender, RoutedEventArgs e)
        {
            View.SendMessage();
        }
    }
}
