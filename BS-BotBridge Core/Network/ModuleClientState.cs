using System;
using System.Net.Sockets;
using System.Threading;

namespace BSBBCore.Network
{
    internal class ModuleClientState : IDisposable
    {
        public string Identifier { get; private set; }
        public TcpClient Client { get; private set; }
        public byte[] Buffer { get; private set; }
        public Timer HeartbeatTimer { get; private set; }
        public DateTime LastPongRecieved { get; set; }
        public ModuleClientState(string identifier, TcpClient client, byte[] buffer, TimerCallback timerCallback, TimeSpan heartbeatInterval) 
        { 
            Identifier = identifier;
            Client = client;
            Buffer = buffer;
            HeartbeatTimer = new Timer(timerCallback, this, heartbeatInterval, heartbeatInterval);
        }

        public void Dispose()
        {
            Client?.Close();
            HeartbeatTimer?.Dispose();
            Client = null;
            HeartbeatTimer = null;
        }
    }
}
