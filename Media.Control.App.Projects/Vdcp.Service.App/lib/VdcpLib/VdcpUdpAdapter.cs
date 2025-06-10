using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace VdcpService.lib
{
    public class VdcpUdpAdapter
    {
        public delegate void VdcpReciveDataAgre(byte[] recData,int count);
        public event VdcpReciveDataAgre ReciveData;


        private Socket ServerSocket = null;
        private Socket ClientSocket = null;

        private string sendAddress = string.Empty;
        private string SendAddress { get { return sendAddress; } set 
            {
                if(sendAddress != value)
                {
                    Client(value, Port);
                }

                sendAddress = value;
            } }
        private int Port { get; set; }
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;
        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }
        public VdcpUdpAdapter()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }
        public void Start(int port)
        {
            Port = port;
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            ServerSocket.Bind(ep);
            Receive();
            bReceive = true;
        }

        bool bReceive = false;
        public void Close()
        {
            bReceive = false;
            System.Threading.Thread.Sleep(100);
            ServerSocket.Close();
            ClientSocket.Close();
        }

        public void Client(string address, int port)
        {
            if ( ((IPEndPoint)ClientSocket.RemoteEndPoint)?.Address.ToString() != address
                || ((IPEndPoint)ClientSocket.RemoteEndPoint)?.Address.ToString() == string.Empty)
            {
                ClientSocket.Connect(IPAddress.Parse(address), port);
            }
        }
        public void Send(byte[] SendData)
        {
            try
            {   
                ClientSocket.BeginSend(SendData, 0, SendData.Length, SocketFlags.None, (ar) =>
                {
                    if (ClientSocket != null)
                    {
                        State so = (State)ar.AsyncState;
                        int bytes = ClientSocket.EndSend(ar);
                        Debug.WriteLine("SEND: {0}, {1}", bytes, SendData);
                    }
                }, state);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Receive()
        {
            ServerSocket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                if (ServerSocket != null && bReceive)
                {
                    State so = (State)ar.AsyncState;
                    int bytes = ServerSocket.EndReceiveFrom(ar, ref epFrom);
                    ServerSocket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);

                    if (ReciveData != null)
                    {
                        string ipAddress = ((IPEndPoint)epFrom).Address.ToString();
                        if (SendAddress != ipAddress)
                            SendAddress = ipAddress;

                        byte[] rxBuffer = new byte[bytes];
                        Array.Copy(so.buffer, 0, rxBuffer, 0, bytes);

                        var task = Task.Run(() => ReciveData(rxBuffer, bytes));
                        task.Wait();
                      //  Console.WriteLine("RECV: {0}: {1}, {2}", ipAddress, bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));

                    }
                }
            }, state);

        }

    }

 
}
