using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VdcpService.lib
{
    public class UdpServerAdapter
    {
        public delegate void VdcpReciveDataAgre(string recData);
        public event VdcpReciveDataAgre ReciveData;

        public ManualResetEvent allDone = new ManualResetEvent(false);

        public string TruncateLeft(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        public void StartListening(int port)
        {
            byte[] bytes = new byte[1024];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                //listener.Connect(localEndPoint);

              //  listener.Bind(localEndPoint);
              //  listener.Listen(1);

                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("Waiting for a connections...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("StartListening[SocketException] Error : {0} ", se.Message.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("StartListening[Exception] Error : {0} ", ex.Message.ToString());
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        public void ReadCallback(IAsyncResult ar)
        {
            string content = string.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.Default.GetString(state.buffer, 0, bytesRead));

                content = state.sb.ToString();
                if (content.IndexOf("<eof>") > -1)
                {
                    ReciveData(content);

                    content = TruncateLeft(content, content.Length - 5);
                  //  Console.WriteLine("Read {0} bytes from socket \nData : {1}", content.Length, content);
                    //Send(handler, content);
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }
        public void Send(string data)
        {
            StateObject state = new StateObject();
            Socket handler = state.workSocket;
            Send(handler, data);
        }
        private void Send(Socket handler, string data)
        {
            byte[] byteData = Encoding.Default.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = ar.AsyncState as Socket;
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (SocketException se)
            {
                Console.WriteLine("SendCallback[SocketException] Error : {0} ", se.Message.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendCallback[Exception] Error : {0} ", ex.Message.ToString());
            }
        }

    }
}
