using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Vdcp.Control.Client
{
    public class VdcpUdpAdapter
    {
        public delegate void VdcpReciveDataAgre(byte[] recData, int count);
        public event VdcpReciveDataAgre ReciveData;


        private Socket ServerSocket = null;
        private Socket ClientSocket = null;

        private string sendAddress = string.Empty;
        private string SendAddress
        {
            get { return sendAddress; }
            set
            {
                if (sendAddress != value)
                {
                    Client(value, Port);
                }

                sendAddress = value;
            }
        }
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
            if (ServerSocket != null)
            {
                ServerSocket.Shutdown(SocketShutdown.Both);
                ServerSocket.Close();
            }

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ClientSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        }
        public void Start(int port)
        {
            Port = port;
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            ServerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            ServerSocket.Bind(ep);
            Receive();
        }

        public void Close()
        {
            ServerSocket.Close(); // .Disconnect(true);
            ClientSocket.Close(); //.Disconnect(true);
        }

        public void Client(string address, int port)
        {
            if (((IPEndPoint)ClientSocket.RemoteEndPoint)?.Address.ToString() != address
                || ((IPEndPoint)ClientSocket.RemoteEndPoint)?.Address.ToString() == string.Empty)
            {
                //if (ClientSocket.Connected)
                //    ClientSocket.Disconnect(false);

                ClientSocket.Connect(IPAddress.Parse(address), port);
            }
        }
        public bool Send(byte[] SendData)
        {
            bool result = false;
            try
            {
                ClientSocket.BeginSend(SendData, 0, SendData.Length, SocketFlags.None, (ar) =>
                {
                    State so = (State)ar.AsyncState;
                    int bytes = ClientSocket.EndSend(ar);
                  //  Debug.WriteLine("SEND: {0}, {1}", bytes, SendData);
                }, state);
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public byte[] GetReceive()
        {
            byte[] rebuffer = null;

            ServerSocket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = ServerSocket.EndReceiveFrom(ar, ref epFrom);
                ServerSocket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);

                if (ReciveData != null)
                {
                    rebuffer = new byte[so.buffer.Length];
                    Array.Copy(so.buffer, 0, rebuffer, 0, so.buffer.Length);
                }

            }, state);

            return rebuffer;
        }

        private void Receive()
        {


            ServerSocket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {

                try
                {
                    State so = (State)ar.AsyncState;

                    int bytes = ServerSocket.EndReceiveFrom(ar, ref epFrom);
                    ServerSocket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);

                    if (ReciveData != null)
                    {
                        if (SendAddress != ((IPEndPoint)epFrom).Address.ToString())
                            SendAddress = ((IPEndPoint)epFrom).Address.ToString();

                        byte[] rxBuffer = new byte[bytes];
                        Array.Copy(so.buffer, 0, rxBuffer, 0, bytes);
                        var task = Task.Run(() => ReciveData(rxBuffer, bytes));
                        task.Wait();
                    }

                }
                catch { }

                //Debug.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));

            }, state);

        }

    }

}
