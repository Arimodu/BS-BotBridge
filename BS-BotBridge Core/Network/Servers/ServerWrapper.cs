using System;
using BSBBCore.Network.Packets;

namespace BSBBCore.Network.Servers
{
    public class ServerWrapper : IDisposable
    {
        private readonly StandaloneLogger _logger;
        private ModuleServer _moduleServer;
        private GameServer _gameServer;
        private bool disposedValue;

        public ServerWrapper(Action<string> log, int logLevel = (int)LogLevel.Info)
        {
            _logger = new StandaloneLogger(log, (LogLevel)logLevel);
            _moduleServer = new ModuleServer(_logger);
            _gameServer = new GameServer(_logger);

            _moduleServer.OnDataReceived += ModuleServer_OnDataReceived;
            _gameServer.OnDataReceived += GameServer_OnDataReceived;
        }

        public void Start()
        {
            _logger.Info("Starting BSBB server components...");
            _moduleServer.Start();
            _gameServer.Start();
            _logger.Info("BSBB server components started");
        }

        public void Stop()
        {
            _logger.Info("Stopping BSBB server components...");
            _moduleServer.Stop();
            _gameServer.Stop();
            _logger.Info("BSBB server components stopped");
        }

        private void GameServer_OnDataReceived(Packet packet)
        {
            switch (packet.CorePacketType)
            {
                case PacketType.Heartbeat:
                    // This shouldnt fire here, just in case return right away
                    return;
                case PacketType.CoreData:
                    // Unused at the time being
                    break;
                case PacketType.ModuleData:
                    _moduleServer.Send(packet, packet.TargetModule);
                    break;
                default:
                    // This is evel less likely to fire, again, just in case return right away
                    return;
            }
        }

        private void ModuleServer_OnDataReceived(Packet packet)
        {
            switch (packet.CorePacketType)
            {
                case PacketType.Heartbeat:
                    // This shouldnt fire here, just in case return right away
                    return;
                case PacketType.CoreData:
                    // Unused at the time being
                    break;
                case PacketType.ModuleData:
                    _gameServer.Send(packet);
                    break;
                default:
                    // This is evel less likely to fire, again, just in case return right away
                    return;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _moduleServer.OnDataReceived -= ModuleServer_OnDataReceived;
                    _gameServer.OnDataReceived -= GameServer_OnDataReceived;
                    _moduleServer.Stop();
                    _moduleServer.Dispose();
                    _moduleServer = null;
                    _gameServer.Stop();
                    _gameServer.Dispose();
                    _gameServer = null;
                }

                disposedValue = true;
            }
        }

        ~ServerWrapper()
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
