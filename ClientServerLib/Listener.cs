using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ClientServerLib
{
    public class Listener
    {
        private TcpListener _listener;
        public bool isListening { get; private set; }

        public delegate void ClientAcceptedDelegate(Client client);
        public event ClientAcceptedDelegate ClientAccepted;

        public Listener(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            if (isListening)
                return;
            isListening = true;

            _listener.Start();

            _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
        }

        public void Stop()
        {
            if (!isListening)
                return;
            isListening = false;

            _listener.Stop();
        }

        public void AcceptTcpClient(IAsyncResult ar)
        {
            TcpClient tcpClient = _listener.EndAcceptTcpClient(ar);

            _listener.BeginAcceptTcpClient(AcceptTcpClient, null);

            ClientAccepted?.Invoke(new Client(tcpClient));
        }
    }
}
