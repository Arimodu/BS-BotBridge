using BSBBCore.Network.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BSBBCore.SharedValues;

namespace BSBBCore.Network.Servers
{
    internal class ModuleServer : IDisposable
    {
        private static IPAddress Address => IPAddress.Loopback;
        private static int Port => 7228;
        private TcpListener _listener;
        private bool disposedValue;
        private readonly ConcurrentDictionary<string, ModuleClientState> _moduleClients;
        private readonly Thread _thread;
        private readonly StandaloneLogger _logger;
        private readonly CancellationTokenSource _cts;

        public event Action<Packet> OnDataReceived;

        internal ModuleServer(StandaloneLogger logger)
        {
            _logger = logger;
            _cts = new CancellationTokenSource();
            _moduleClients = new ConcurrentDictionary<string, ModuleClientState>();
            _listener = new TcpListener(Address, Port);

            _thread = new Thread(() => MainServerLoop(_cts.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "BSBB Module Server Thread"
            };
        }

        public void Start()
        {
            _logger.Info("Starting BSBB module server...");
            _listener.Start();
            _thread.Start();
            _logger.Info("Started BSBB module server");
        }

        public void Stop()
        {
            _cts.Cancel();
            _logger.Info("Stopping BSBB game server...");
            _logger.Debug("Stopping listening thread...");
            while (_thread.IsAlive)
            {
                Thread.Sleep(10);
            }
            _logger.Debug("Disconnecting clients...");
            foreach (var identifier in _moduleClients.Keys)
            {
                if (_moduleClients.TryRemove(identifier, out var clientState))
                {
                    clientState.Dispose();
                    _logger.Debug($"Disconnected client with identifier {identifier}");
                }
                else _logger.Warn($"Failed to remove {identifier} client. Possibly already disconnected. Ignoring...");
            }
            _logger.Info("Stopped BSBB game server");
        }

        private async void MainServerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var connectingClient = await _listener.AcceptTcpClientAsync();

                    // Get the identifier of the module client from the incoming data
                    var moduleIdentifier = await ReadIdentifier(connectingClient);
                    if (string.IsNullOrEmpty(moduleIdentifier))
                    {
                        _logger.Warn($"Invalid module client connection {connectingClient.Client.RemoteEndPoint}");
                        CloseConnection(connectingClient);
                        continue;
                    }

                    // If a module client with the same identifier is already connected, refuse the new connection
                    if (_moduleClients.TryGetValue(moduleIdentifier, out var moduleClientState) && moduleClientState.Client.Connected)
                    {
                        _logger.Warn($"Refusing module client connection {connectingClient.Client.RemoteEndPoint}. Identifier '{moduleIdentifier}' is already connected");
                        CloseConnection(connectingClient);
                        continue;
                    }

                    // Accept the connection
                    var buffer = new byte[4096];
                    var state = new ModuleClientState(moduleIdentifier, connectingClient, buffer, Heartbeat, HeartbeatInterval)
                    {
                        LastPongRecieved = DateTime.UtcNow
                    };
                    if (_moduleClients.TryAdd(moduleIdentifier, state))
                    {
                        _logger.Info($"Module client connected with identifier '{moduleIdentifier}' from {connectingClient.Client.RemoteEndPoint}");
                        var stream = connectingClient.GetStream();
                        stream.BeginRead(buffer, 0, buffer.Length, ReceiveCallback, state);
                    }
                    else
                    {
                        _logger.Error($"Failed to add module client with identifier '{moduleIdentifier}' from {connectingClient.Client.RemoteEndPoint}");
                        CloseConnection(connectingClient);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"Failed to accept module connection: {e.Message}");
                }
            }

            // Cleanup after ourselves
            try
            {
                _listener.Stop();
            }
            catch (Exception e)
            {
                _logger.Error($"Error while stopping module server: {e.Message}");
            }
            _logger.Info("Stopped BSBB module server");
        }

        private void Heartbeat(object stateObject)
        {
            ModuleClientState state = stateObject as ModuleClientState;
            var client = state.Client;
            var lastPongRecieved = state.LastPongRecieved;

            var now = DateTime.UtcNow;

            // Check if we received a pong recently
            if ((now - lastPongRecieved) > HeartbeatTimeout)
            {
                _logger.Warn($"Closing connection due to heartbeat timeout: {client.Client.RemoteEndPoint}");
                CloseConnection(client);
                return;
            }

            // Send heartbeat packet
            _logger.Debug($"Sending Heartbeat to Client");
            string json = JsonConvert.SerializeObject("Ping");
            var data = Encoding.UTF8.GetBytes(json);
            var packet = new Packet("Core", data, type: PacketType.Heartbeat);
            Send(packet, state);
        }

        private async Task<string> ReadIdentifier(TcpClient client)
        {
            var stream = client.GetStream();
            var buffer = new byte[1024];

            // Give the client some time to send the Ident packet
            await Task.Delay(50);

            // Read the incoming data until and parse them as an IdentPacket
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) return null;
            string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            IdentPacket packet = JsonConvert.DeserializeObject<IdentPacket>(json);
            return packet.Identifier;
        }

        private void CloseConnection(TcpClient client)
        {
            _logger.Debug($"Closing connection: {client.Client.RemoteEndPoint}");
            client?.Close();
        }

        private void CloseConnection(ModuleClientState state)
        {
            var client = state.Client;
            _logger.Debug($"Closing connection: {client.Client.RemoteEndPoint}");
            state.HeartbeatTimer?.Dispose();
            client?.Close();
            state.Dispose();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = ar.AsyncState as ModuleClientState;
                var identifier = state.Identifier;
                var client = state.Client;
                var stream = client.GetStream();
                var buffer = state.Buffer;

                int bytesRead = stream.EndRead(ar);
                _logger.Debug($"Recieved {bytesRead} bytes from module with identifier {identifier}");

                if (bytesRead > 0)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Packet packet = JsonConvert.DeserializeObject<Packet>(json);
                    _logger.Debug($"Recieved data parsed as {packet.CorePacketType} for {packet.TargetModule} module");

                    switch (packet.CorePacketType)
                    {
                        case PacketType.Heartbeat:
                            _logger.Debug($"Recieved heartbeat from Client");
                            state.LastPongRecieved = DateTime.UtcNow;
                            break;
                        case PacketType.CoreData:
                            break;
                        case PacketType.ModuleData:
                            OnDataReceived?.Invoke(packet);
                            break;
                        default:
                            _logger.Warn($"Recieved malformed packet from {client.Client.RemoteEndPoint}. Discarding...");
                            break;
                    }
                }
                else if (bytesRead == 0)
                {
                    _logger.Info($"Recieved 0 bytes from stream. Assuming client disconnected");
                    CloseConnection(state);
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to receive data from client: {e.Message}");
            }
        }

        public void Send(Packet packet, ModuleClientState state)
        {
            var client = state.Client;
            var stream = client.GetStream();

            _logger.Debug($"Sending {packet.CorePacketType} for {packet.TargetModule} module");
            try
            {
                string json = JsonConvert.SerializeObject(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);

                _logger.Debug($"Sending {data.Length} bytes of data to {client.Client.RemoteEndPoint}");
                lock (stream)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to send data to client: {e.Message}");
            }
        }

        public void Send(Packet packet, string identifier)
        {
            if (_moduleClients.TryGetValue(identifier, out var state)) Send(packet, state);
            else _logger.Error($"Failed to send packet to {identifier}. No such module");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cts.Dispose();
                    foreach (var clientState in _moduleClients.Values)
                    {
                        clientState.Dispose();
                    }
                    _listener.Stop();
                    _thread.Join();
                    _logger.Info("Disposed BSBB module server resources");
                }

                disposedValue = true;
            }
        }

        ~ModuleServer()
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
