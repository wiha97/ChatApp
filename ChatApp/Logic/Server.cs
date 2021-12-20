using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Logic
{
    public static class Server
    {
        private static byte[] buffer = new byte[1024];
        private static List<Socket> sockets = new List<Socket>();
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<string> clients = new List<string>();
        private static Socket sendSocket;

        public static void Start(int port)
        {
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(new AsyncCallback(Accept), null);
            
            View.SetTopLab($"Server started on port {port}");
            View.IsHost = true;
        }

        private static void Accept(IAsyncResult ar)
        {
            Socket socket = serverSocket.EndAccept(ar);
            sockets.Add(socket);
            sendSocket = socket;

            string client = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            clients.Add(client);
            View.ConnectedTo = View.CheckName(client);
            View.SetTopLab($"Connected to {View.ConnectedTo} ({client})");
            
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), socket);

        }

        private static void Receive(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                int received = socket.EndReceive(ar);

                byte[] data = new byte[received];
                Array.Copy(buffer, data, received);

                string message = Encoding.UTF8.GetString(data);

                if (!View.IsRequest(message))
                {
                    View.ReceiveMessage(message);
                }

                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), socket);
            }
            catch (Exception e)
            {
                View.ReceiveMessage(e.Message);
            }
        }

        public static void Send(string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            sendSocket.BeginSend(msg, 0, msg.Length, SocketFlags.None, new AsyncCallback(SendCall), sendSocket);
        }

        private static void SendCall(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }
    }
}
