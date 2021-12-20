using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatApp.Logic
{
    public static class Client
    {
        private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public static void Connect(string ips, int port)
        {
            IPAddress ip = IPAddress.Parse(ips);

            while (!clientSocket.Connected)
            {
                try
                {
                    clientSocket.Connect(ip, port);
                    View.ConnectedTo = View.CheckName(ips);
                    View.SetTopLab($"Connected to {View.ConnectedTo} ({ips})");
                    ReceiveMessage();
                }
                catch (Exception e)
                {
                    View.SetTopLab(e.Message);
                }
            }
        }

        public static string SendMessage(string message)
        {
            string msg = "";
            try
            {
                msg = message;
                byte[] buffer = Encoding.UTF8.GetBytes(msg);
                clientSocket.Send(buffer);
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            return msg;
        }
        private static byte[] recBuff = new byte[1024];
        public static void ReceiveMessage()
        {
            try
            {
                clientSocket.BeginReceive(recBuff, 0, recBuff.Length, SocketFlags.None, new AsyncCallback(Receive), clientSocket);
            }
            catch (Exception e)
            {
                View.ReceiveMessage(e.Message);
            }
        }

        private static void Receive(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int received = socket.EndReceive(ar);

                if (received > 0)
                {

                    byte[] data = new byte[received];
                    Array.Copy(recBuff, data, received);

                    string message = Encoding.UTF8.GetString(data);

                    if (!View.IsRequest(message))
                    {
                        View.ReceiveMessage(message);
                    }
                    clientSocket.BeginReceive(recBuff, 0, recBuff.Length, SocketFlags.None, new AsyncCallback(Receive), clientSocket);
                }
                else
                {
                    View.SetTopLab("Disconnected");
                    clientSocket.Close();
                }
            }
            catch
            {
                View.SetTopLab("Disconnected");
                clientSocket.Close();
            }
        }
    }
}
