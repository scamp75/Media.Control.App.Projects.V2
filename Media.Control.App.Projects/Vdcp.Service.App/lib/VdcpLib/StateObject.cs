﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace VdcpService.lib
{
    public class StateObject
    {
        public Socket workSocket = null;
        // Size of receive buffer
        public const int BufferSize = 1024;
        // Receive buffer
        public byte[] buffer = new byte[BufferSize];
        // Received data string
        public StringBuilder sb = new StringBuilder();

    }
}
