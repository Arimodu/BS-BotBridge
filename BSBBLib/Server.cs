using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using BSBBLib.Packets;
using static BSBBLib.SharedValues;

namespace BSBBLib
{
    public class Server
    {
        private static IPAddress Address => IPAddress.Loopback;
        private static int Port => 7227;
        private readonly TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly byte[] _buffer = new byte[4096];
        private readonly Thread _thread;
        private readonly Action<string> _logger;
        private readonly CancellationTokenSource _cts;
        private Timer _hearbeatTimer;
        private DateTime _lastPongReceived;

        public event Action<Packet> OnDataReceived;

        public Server(Action<string> log)
        {
            _logger = log;
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(Address, Port);
            _thread = new Thread(() => Run(_cts.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "BSBB Server Thread"
            };
            _logger.Invoke("Starting BSBB server...");
            _listener.Start();
            _thread.Start();
        }

        public void Stop()
        {
            _cts.Cancel();
            _logger.Invoke("Stopping BSBB server...");
            while (_thread.IsAlive)
            {
                Thread.Sleep(10);
            }
        }

        private async void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var connectingClient = await _listener.AcceptTcpClientAsync();
                    if (_client == null)
                    {
                        AcceptConnection(connectingClient);
                        continue;
                    }

                    if (!_client.Connected)
                    {
                        AcceptConnection(connectingClient);
                        continue;
                    }

                    _logger.Invoke($"Refusing connection {connectingClient.Client.RemoteEndPoint}");
                    CloseConnection(connectingClient);
                }
                catch (Exception e)
                {
                    _logger.Invoke($"Failed to accept client connection: {e.Message}");
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
                _logger.Invoke($"Error while stopping server: {e.Message}");
            }
            _logger.Invoke("Stopped BSBB server");
        }

        private void AcceptConnection(TcpClient client)
        {
            _client = client;
            _logger.Invoke($"Client connected from {_client.Client.RemoteEndPoint}");
            _stream = _client.GetStream();
            _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);

            // Start heartbeat
            _hearbeatTimer = new Timer(Heartbeat, null, HeartbeatInterval, HeartbeatInterval);
            _lastPongReceived = DateTime.UtcNow;
        }

        private void CloseConnection(TcpClient client)
        {
            _logger.Invoke($"Closing connection: {client.Client.RemoteEndPoint}");
            _hearbeatTimer.Dispose();
            client?.Close();
        }

        private void Heartbeat(object state)
        {
            var now = DateTime.UtcNow;

            // Check if we received a pong recently
            if ((now - _lastPongReceived) > HeartbeatTimeout)
            {
                _logger.Invoke($"Closing connection due to heartbeat timeout: {_client.Client.RemoteEndPoint}");
                CloseConnection(_client);
                return;
            }

            // Send heartbeat packet
            _logger.Invoke($"Sending Heartbeat to Client");
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
                _logger.Invoke($"Recieved {bytesRead} bytes from {_client.Client.RemoteEndPoint}");
                if (bytesRead > 0)
                {
                    string json = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                    Packet packet = JsonConvert.DeserializeObject<Packet>(json);
                    _logger.Invoke($"Recieved data parsed as {packet.CorePacketType} for {packet.TargetModule} module");

                    // Handle heartbeat
                    if (packet.CorePacketType == PacketType.Heartbeat)
                    {
                        _logger.Invoke($"Recieved heartbeat from Client");
                        _lastPongReceived = DateTime.UtcNow;
                    }
                    else if (packet.CorePacketType == PacketType.CoreData)
                    {
                        // Whatever the fuck I might use this in the future
                        // Does nohting for the time being
                    }
                    else if (packet.CorePacketType == PacketType.ModuleData)
                    {
                        OnDataReceived?.Invoke(packet);
                    }
                    else
                    {
                        _logger.Invoke($"Recieved malformed packet from {_client.Client.RemoteEndPoint}. Discarding...");
                    }
                }
                else if (bytesRead == 0)
                {
                    _logger.Invoke($"Recieved 0 bytes from stream. Assuming client disconnected");
                    CloseConnection(_client);
                    return;
                }
                _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                _logger.Invoke($"Failed to receive data from client: {e.Message}");
            }
        }

        public void Send(Packet packet)
        {
            _logger.Invoke($"Sending {packet.CorePacketType} for {packet.TargetModule} module");
            try
            {
                string json = JsonConvert.SerializeObject(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);

                _logger.Invoke($"Sending {data.Length} bytes of data to {_client.Client.RemoteEndPoint}");
                lock (_stream)
                {
                    _stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                _logger.Invoke($"Failed to send data to client: {e.Message}");
            }
        }
    }
}
