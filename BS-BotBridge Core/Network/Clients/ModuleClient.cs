using BSBBCore.Interfaces;
using BSBBCore.Network.Packets;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BSBBCore.SharedValues;

namespace BSBBCore.Network.Clients
{
    public class ModuleClient : IClient
    {
        private static readonly IPAddress Address = IPAddress.Loopback;
        private static readonly int Port = 7228;
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _buffer;
        private ConnectionState _state;
        private readonly StandaloneLogger _logger;
        private Timer _pingTimer;
        private DateTime _lastPingReceived;

        public ConnectionState State { 
            get => _state; 
            private set
            {
                if (_state == value) return;
                _state = value;
                OnStateChanged?.Invoke(value);
            } 
        }

        public event Action<ConnectionState> OnStateChanged;

        public ModuleClient(StandaloneLogger logger)
        {
            _client = new TcpClient();
            _logger = logger;
        }

        public void Start()
        {
            _ = StartAsync();
        }

        public async Task StartAsync()
        {
            if (_client.Connected || State == ConnectionState.Connecting) return;

            // If we are errored, dont reconnect until error is resolved
            if (State == ConnectionState.Errored) return;

            State = ConnectionState.Connecting;
            _logger.Info("Connecting to BSBB server...");

            try
            {
                await _client.ConnectAsync(Address, Port);
                _stream = _client.GetStream();
                _buffer = new byte[_client.ReceiveBufferSize];
                _stream.BeginRead(_buffer, 0, _buffer.Length, RecieveCallback, null);
                State = ConnectionState.Connected;
                _logger.Info($"Successfully connected to BSBB server");

                // Start heartbeat timer and reset time
                _pingTimer = new Timer(CheckHeartbeat, null, HeartbeatInterval, HeartbeatInterval);
                _lastPingReceived = DateTime.UtcNow;
            }
            catch (SocketException e)
            {
                _logger.Error($"Failed to connect to server: {e.Message}");

                State = ConnectionState.Errored;
            }
        }

        internal void Disconnect()
        {
            _pingTimer.Dispose();
            _client.Close();
            State = ConnectionState.Disconnected;

            _logger.Info("BSBB Client disconnected");

            // Prepare new TcpClient for the next connection
            _client = new TcpClient();
        }

        private void CheckHeartbeat(object state)
        {
            var now = DateTime.UtcNow;

            // Check if we received a pong recently
            if (now - _lastPingReceived > HeartbeatTimeout)
            {
                _logger.Error($"Closing connection due to heartbeat timeout: {_client.Client.RemoteEndPoint}");
                Disconnect();
            }
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = _stream.EndRead(ar);
                _logger.Debug($"Recieved {bytesRead} bytes from {_client.Client.RemoteEndPoint}");
                string json = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                Packet packet = JsonConvert.DeserializeObject<Packet>(json);
                _logger.Debug($"Recieved data parsed as {packet.CorePacketType} for {packet.TargetModule} module");

                if (packet.CorePacketType == PacketType.Heartbeat) SendHeartbeat();
                else if (packet.CorePacketType == PacketType.CoreData)
                {
                    // Whatever the fuck I might use this for in the future
                    // Does nohting for the time being
                }
                else if (packet.CorePacketType == PacketType.ModuleData) 
                { 
                    
                }
                else _logger.Error("Recieved malformed packet from server");

                _stream.BeginRead(_buffer, 0, _buffer.Length, RecieveCallback, null);
            }
            catch (Exception e)
            {
                if (State == ConnectionState.Disabled) return;
                _logger.Error($"Failed to receive data from server: {e.Message}");
                State = ConnectionState.Errored; // Not exactly sure what this can throw, so I wont put socket realoding here
            }
        }

        private void SendHeartbeat()
        {
            // Since we recieved a ping, set last ping to now
            _lastPingReceived = DateTime.Now;

            // Respond to heartbeat packet
            string json = JsonConvert.SerializeObject("Pong");
            var data = Encoding.UTF8.GetBytes(json);
            var packet = new Packet("Core", data, type: PacketType.Heartbeat);
            Send(packet);
        }

        // Not used in this context
        public void Reconnect() { }

        public void Send(Packet packet)
        {
            try
            {
                _logger.Debug($"Sending {packet.CorePacketType} for {packet.TargetModule} module");
                string json = JsonConvert.SerializeObject(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);
                _logger.Debug($"Sending {data.Length} bytes of data to {_client.Client.RemoteEndPoint}");

                lock (_stream)
                {
                    _stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                if (State == ConnectionState.Disabled) return;
                _logger.Error($"Failed to send data to server: {e.Message}");
                State = ConnectionState.Errored; // Not exactly sure what this can throw, so I wont put socket realoding here
            }
        }
    }
}
