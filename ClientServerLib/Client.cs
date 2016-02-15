using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ClientServerLib
{
    public class Client
    {
        public bool Connected => tcpClient.Connected;
        public const int BUFFER_SIZE = 1024;
        public TcpClient tcpClient { get; private set; }
        private byte[] lenBuffer;
        private byte[] receiveBuffer;
        private int toReceive;
        private MemoryStream ms;
        public NetworkStream networkStream => tcpClient.GetStream();

        public delegate void PacketReceiveEventHandler(Client sender, PacketReceivedEventArgs e);
        public delegate void ClientDisconnectedEventHandler(Client e);

        public event PacketReceiveEventHandler PacketReceived;
        public event ClientDisconnectedEventHandler ClientDisconnected;

        public class PacketReceivedEventArgs : EventArgs
        {
            public BinaryReader Reader { get; private set; }
            public int PacketLength => (int)Reader.BaseStream.Length;
            public PacketReceivedEventArgs(BinaryReader reader)
            {

                Reader = reader;
            }
        }

        public void Send(byte[] packetData)
        {
            networkStream.Write(packetData, 0, packetData.Length);
        }

        public void Send(Packet packet) => Send(packet.GetBytes());

        public Client()
        {
            ms = new MemoryStream();
            lenBuffer = new byte[4];
            receiveBuffer = new byte[1024];
        }

        public Client(TcpClient tcpClient) : this()
        {
            this.tcpClient = tcpClient;
            networkStream.BeginRead(lenBuffer, 0, lenBuffer.Length, Receive, null);
        }

        public void Connect(string hostname, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(hostname, port);
                networkStream.BeginRead(lenBuffer, 0, lenBuffer.Length, Receive, null);
            }
            catch (Exception ex) { throw new Exception("Could not connect to remote host."); }
        }

        public void Receive(IAsyncResult ar)
        {
            try
            {
                int len = networkStream.EndRead(ar);          
                if (len <= 0)
                {
                    Close();
                    return;
                }
                toReceive = BitConverter.ToInt32(lenBuffer, 0);
                ms = new MemoryStream();
                ReceivePacket(toReceive);
            }
            catch  { Close(); }           
        }

        public void ReceivePacket(int toReceive)
        {
            try
            {
                int len = networkStream.Read(receiveBuffer, 0, Math.Min(toReceive, receiveBuffer.Length));
                if (len <= 0)
                {
                    Close();
                    return;
                }
                toReceive -= len;
                ms.Write(receiveBuffer, 0, len);
                if (toReceive > 0)
                {
                    ReceivePacket(toReceive);
                    return;
                }
                ms.Position = 0L;
                PacketReceived?.Invoke(this, new PacketReceivedEventArgs(new BinaryReader(ms)));
                ms.Close();
            }
            catch (SocketException) { Close(); }
            catch (Exception) {  /*Don't handle*/ }
            networkStream.BeginRead(lenBuffer, 0, lenBuffer.Length, Receive, null);
        }

        public void Close()
        {
            ms.Close();
            ms.Dispose();
            tcpClient.Close();
            ClientDisconnected?.Invoke(this);
        }
    }
}
