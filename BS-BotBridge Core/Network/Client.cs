using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BSBBCore;
using BSBBCore.Configuration;
using BSBBCore.Network.Packets;
using BSBBCore.Managers;
using BSBBCore.Interfaces;
using SiraUtil.Logging;
using Zenject;
using static BSBBCore.SharedValues;

namespace BS_BotBridge_Core.Network
{
    // Todo: Implements better socket error handling
    // Maybe leave it up to the sending module??
    public class Client : IClient, IInitializable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _buffer;
        private string _address;
        private int _port;
        private ConnectionState _state;
        private readonly SiraLog _logger;
        private readonly BSBBModuleManager _moduleManager;
        private readonly BSBBCoreConfig _config;
        private Timer _pingTimer;
        private DateTime _lastPingReceived;

        public event Action<ConnectionState> OnStateChanged;
        public ConnectionState State
        {
            get => _state;
            private set
            {
                _state = value;
                OnStateChanged?.Invoke(value);
            }
        }

        internal Client(SiraLog siraLog, BSBBCoreConfig config, BSBBModuleManager moduleManager)
        {
            _logger = siraLog;
            _client = new TcpClient();
            _config = config;
            _address = _config.ServerAddress;
            _port = _config.ServerPort;

            if (_config.ConnectionEnabled) State = ConnectionState.Disconnected;
            else State = ConnectionState.Disabled;

            // Yes, this creates a circular dependency, do I care? NO.
            // Fuck maintainability, this is my bonfire
            _moduleManager = moduleManager;


            _config.OnChanged += Config_OnChanged;
        }

        public void Initialize()
        {
            if (_config.ConnectionEnabled) Start();
        }

        private void Config_OnChanged()
        {
            // First check if the parameters have changed
            bool isInstanceDirty = false;
            if (_address != _config.ServerAddress || _port != _config.ServerPort)
            {
                _address = _config.ServerAddress;
                _port = _config.ServerPort;
                isInstanceDirty = true;

                // Log only when we want to be connected, otherwise there is no point to reloading
                if (_config.ConnectionEnabled) _logger.Warn("BSBB Client instance marked dirty. It will be reloaded soon");
            }

            // If we are connected and we dont want to be, disconnect
            // Or if marked dirty and connected
            if (_client.Connected && !_config.ConnectionEnabled || isInstanceDirty && _client.Connected)
                Disconnect();

            // If we are disconnected and want to connect, reconnect
            if (!_client.Connected && _config.ConnectionEnabled)
            {
                // We want to connect, so we are not disabled anymore
                if (State == ConnectionState.Disabled) State = ConnectionState.Disconnected;
                Reconnect();
            }
        }

        // Patchover work so I dont run init on UI thread
        internal void Start()
        {
            _ = StartAsync();
        }

        public void Reconnect()
        {
            if (State == ConnectionState.Disabled || _client.Connected) return;
            if (State == ConnectionState.Errored) State = ConnectionState.Disconnected;
            _ = StartAsync();
        }

        private async Task StartAsync()
        {
            if (_client.Connected || State == ConnectionState.Connecting) return;

            if (!_config.ConnectionEnabled)
            {
                State = ConnectionState.Disabled;
                return;
            }

            // If we are errored, dont reconnect until error is resolved (or game is realoded)
            if (State == ConnectionState.Errored) return;

            State = ConnectionState.Connecting;
            _logger.Info("Connecting to BSBB server...");

            if (_address == null || _port == 0 || _port > 65535)
            {
                State = ConnectionState.Errored;
                _logger.Error($"Failed to connect to server: Malformed address {_address} or port {_port}, please check your configuration");
                return;
            }

            try
            {
                await _client.ConnectAsync(_address, _port);
                _stream = _client.GetStream();
                _buffer = new byte[_client.ReceiveBufferSize];
                _stream.BeginRead(_buffer, 0, _buffer.Length, OnDataReceived, null);
                State = ConnectionState.Connected;
                _logger.Info($"Successfully connected to BSBB server at {_address}:{_port}");

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

        private void OnDataReceived(IAsyncResult result)
        {
            try
            {
                int bytesRead = _stream.EndRead(result);
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
                else if (packet.CorePacketType == PacketType.ModuleData) _moduleManager.FindAndGetModule(packet.TargetModule).RecievePacket(packet);
                else _logger.Error("Recieved malformed packet from server");

                _stream.BeginRead(_buffer, 0, _buffer.Length, OnDataReceived, null);
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
    }
}
