using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BSBBCore.Network.Packets;
using static BSBBCore.SharedValues;

namespace BSBBCore.Network.Servers
{
    internal class GameServer : IDisposable
    {
        private static IPAddress Address => IPAddress.Loopback;
        private static int Port => 7227;
        private readonly TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly byte[] _buffer = new byte[4096];
        private readonly Thread _thread;
        private readonly StandaloneLogger _logger;
        private readonly CancellationTokenSource _cts;
        private Timer _hearbeatTimer;
        private DateTime _lastPongReceived;
        private bool disposedValue;

        public event Action<Packet> OnDataReceived;

        internal GameServer(StandaloneLogger logger)
        {
            _logger = logger;
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(Address, Port);
            _thread = new Thread(() => Run(_cts.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "BSBB Server Thread"
            };
        }

        public void Start()
        {
            _logger.Info("Starting BSBB game server...");
            _listener.Start();
            _thread.Start();
            _logger.Info("Started BSBB game server");
        }

        public void Stop()
        {
            _cts.Cancel();
            _logger.Info("Stopping BSBB game server...");
            while (_thread.IsAlive)
            {
                Thread.Sleep(10);
            }
            _logger.Info("Stopped BSBB game server");
        }

        private async void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    if (_client == null || !_client.Connected)
                    {
                        _client = client;
                        _logger.Info($"Client connected from {_client.Client.RemoteEndPoint}");
                        _stream = _client.GetStream();
                        _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);

                        // Start heartbeat
                        _hearbeatTimer = new Timer(Heartbeat, null, HeartbeatInterval, HeartbeatInterval);
                        _lastPongReceived = DateTime.UtcNow;
                        continue;
                    }

                    _logger.Warn($"Refusing connection {client.Client.RemoteEndPoint}");
                    CloseConnection(client);
                }
                catch (Exception e)
                {
                    _logger.Error($"Failed to accept client connection: {e.Message}");
                    _client = null;
                }

                await Task.Delay(500, token);
            }

            // Cleanup after ourselves
            try
            {
                _listener.Stop();
                _client?.Close();
                _stream?.Dispose();
            }
            catch (Exception e)
            {
                _logger.Error($"Error while stopping server: {e.Message}");
            }
        }

        private void CloseConnection(TcpClient client)
        {
            _logger.Debug($"Closing connection: {client.Client.RemoteEndPoint}");
            _hearbeatTimer.Dispose();
            client?.Close();
        }

        private void Heartbeat(object state)
        {
            var now = DateTime.UtcNow;

            // Check if we received a pong recently
            if ((now - _lastPongReceived) > HeartbeatTimeout)
            {
                _logger.Warn($"Closing connection due to heartbeat timeout: {_client.Client.RemoteEndPoint}");
                CloseConnection(_client);
                return;
            }

            // Send heartbeat packet
            _logger.Debug($"Sending Heartbeat to Client");
            string json = JsonConvert.SerializeObject("Ping");
            var data = Encoding.UTF8.GetBytes(json);
            var packet = new Packet("Core", data, type: PacketType.Heartbeat);
            Send(packet);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = _stream.EndRead(ar);
                _logger.Debug($"Recieved {bytesRead} bytes from {_client.Client.RemoteEndPoint}");
                if (bytesRead > 0)
                {
                    string json = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                    Packet packet = JsonConvert.DeserializeObject<Packet>(json);
                    _logger.Debug($"Recieved data parsed as {packet.CorePacketType} for {packet.TargetModule} module");

                    switch (packet.CorePacketType)
                    {
                        case PacketType.Heartbeat:
                            _logger.Debug($"Recieved heartbeat from Client");
                            _lastPongReceived = DateTime.UtcNow;
                            break;
                        case PacketType.CoreData:
                            // Whatever the fuck I might use this in the future
                            // Does nohting for the time being
                            break;
                        case PacketType.ModuleData:
                            OnDataReceived?.Invoke(packet);
                            break;
                        default:
                            _logger.Warn($"Recieved malformed packet from {_client.Client.RemoteEndPoint}. Discarding...");
                            break;
                    }

                }
                else if (bytesRead == 0)
                {
                    _logger.Info($"Recieved 0 bytes from stream. Assuming client disconnected");
                    CloseConnection(_client);
                    return;
                }
                _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to receive data from client: {e.Message}");
            }
        }

        public void Send(Packet packet)
        {
            _logger.Debug($"Sending {packet.CorePacketType} for {packet.TargetModule} module");
            try
            {
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
                _logger.Error($"Failed to send data to client: {e.Message}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose of managed resources here
                    _hearbeatTimer?.Dispose();
                    _cts?.Dispose();
                    _stream?.Dispose();
                    _client?.Dispose();
                    _listener?.Stop();
                }

                // Free unmanaged resources here
                // No unmanaged resources used, nothing to do here
                disposedValue = true;
            }
        }

        ~GameServer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
