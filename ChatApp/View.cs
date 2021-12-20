using Avalonia;
using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using ChatApp.Logic;
using Avalonia.Media;
using Avalonia.Threading;
using System.IO;

namespace ChatApp
{
    public static class View
    {
        private static MainWindow win;
        private static StackPanel messageStack;
        private static TextBox messageBox;
        private static TextBox connectBox;
        public static string ConnectedTo { get; set; }
        public static bool IsHost { get; set; } = false;

        public static void SetWin(MainWindow window)
        {
            win = window;
            messageStack = win.FindControl<StackPanel>("MessageStack");
            messageBox = win.FindControl<TextBox>("MessageBox");
            connectBox = win.FindControl<TextBox>("ConBox");
        }

        public static async void SetTopLab(string content)
        {
            await Task.Run(() =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    win.FindControl<Label>("TopLab").Content = content;
                });
            });
        }

        private static Label From(string from)
        {
            return new Label()
            {
                Foreground = new SolidColorBrush(Colors.Gray),
                Content = from,
                FontSize = 14
            };
        }
        private static TextBlock Message(string message)
        {
            return new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(10),
                Text = message,
                FontSize = 24
            };
        }
        private static Label Time()
        {
            return new Label()
            {
                Foreground = new SolidColorBrush(Colors.Gray),
                Content = DateTime.Now.ToString("HH:mm"),
                FontSize = 14
            };
        }

        public static void SendMessage()
        {
            if (!messageBox.Text.StartsWith('/'))
            {
                Label from = From("You");
                Label time = Time();
                from.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                time.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                StackPanel stack = MsgStack(messageBox.Text, from, time);
                stack.Background = new SolidColorBrush(Colors.Navy);
                stack.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                messageStack.Children.Add(stack);
            }
            if (IsHost)
            {
                Server.Send(messageBox.Text);
            }
            else
            {
                Client.SendMessage(messageBox.Text);
            }
            messageBox.Clear();
        }

        public static async void ReceiveMessage(string message)
        {
            await Task.Run(() =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Label from = From(ConnectedTo);
                    Label time = Time();
                    from.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                    time.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                    StackPanel stack = MsgStack(message, from, time);
                    stack.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                    stack.Background = new SolidColorBrush(Colors.DarkGreen);
                    messageStack.Children.Add(stack);

                });
            });
        }

        private static StackPanel MsgStack(string msg, Label from, Label time)
        {
            StackPanel stack = new StackPanel();
            stack.Children.Add(from);
            stack.Children.Add(Message(msg));
            stack.Children.Add(time);
            return stack;
        }

        public static bool IsRequest(string message)
        {
            if (message.StartsWith('/'))
            {
                try
                {
                    switch (message[1])
                    {
                        case '?':
                            if (IsHost)
                                Server.Send($"/:{Environment.MachineName}");
                            else
                                Client.SendMessage($"/:{Environment.MachineName}");
                            break;
                        case ':':
                            SaveClient(message.Substring(message.IndexOf(':') + 1));
                            break;
                        default:
                            SetTopLab("???");
                            break;
                    }
                }
                catch(Exception e)
                {
                    SetTopLab(e.Message);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void SaveClient(string name)
        {
            File.AppendAllText("clients", $"{ConnectedTo},{name}{Environment.NewLine}");
        }

        public static string CheckName(string name)
        {
            if (File.Exists("clients"))
            {
                foreach(string line in File.ReadAllLines("clients"))
                {
                    if (line.Contains(name))
                    {
                        name = line.Substring(line.IndexOf(',') + 1);
                    }
                }
            }
            return name;
        }

        public static void Connect()
        {
            try
            {
                string fullIp = connectBox.Text;
                string[] ip = fullIp.Split(':');
                int port = 80;
                if(ip.Length == 2)
                {
                    port = int.Parse(ip[1]);
                }
                SetTopLab($"Connecting to {ip[0]}");
                Client.Connect(ip[0], port);
            }
            catch(Exception e)
            {
                SetTopLab($"{e.Message}");
            }
        }

        public static void Host()
        {
            try
            {
                int port = 80;
                if(connectBox.Text != null)
                {
                    port = int.Parse(connectBox.Text);
                }
                Server.Start(port);
            }
            catch(Exception e)
            {
                SetTopLab($"{e.Message}");
            }
        }
    }
}
