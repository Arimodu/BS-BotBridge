using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using SiraUtil.Logging;
using BS_BotBridge_Core.Configuration;
using BSBBLib;
using BS_BotBridge_Core.Managers;

namespace BS_BotBridge_Core
{
    // Todo: Implements better socket error handling
    // Maybe leave it up to the sending module??
    public class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] buffer;
        private string address;
        private int port;
        private readonly SiraLog logger;
        private readonly BSBBModuleManager _moduleManager;
        private readonly BSBBCoreConfig _config;

        public Client(SiraLog siraLog, BSBBCoreConfig config, BSBBModuleManager moduleManager)
        {
            logger = siraLog;
            client = new TcpClient();
            _config = config;
            address = _config.ServerAddress;
            port = _config.ServerPort;

            // Yes, this creates a circular dependency, do I care? NO.
            // Fuck maintainability, this is my bonfire
            _moduleManager = moduleManager;

            _config.OnChanged += Config_OnChanged;
        }

        private void Config_OnChanged()
        {
            // First check if the parameters have changed
            if (address != _config.ServerAddress) address = _config.ServerAddress;
            if (port != _config.ServerPort) port = _config.ServerPort;

            // If we are connected and we dont want to be, disconnect
            if (client.Connected && !_config.ConnectionEnalbed)
            {
                client.Close();

                // Prepare new TcpClient for the next connection
                client = new TcpClient();
            }

            // If we are disconnected and want to connect, start
            if (!client.Connected && _config.ConnectionEnalbed) Start();
        }

        public void Start()
        {
            if (!_config.ConnectionEnalbed) return;
            if (address == null || port == 0) return;
            if (client.Connected) return;

            try
            {
                client.Connect(address, port);
                stream = client.GetStream();
                buffer = new byte[client.ReceiveBufferSize];
                stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
            }
            catch (SocketException e)
            {
                logger.Error($"Failed to connect to server: {e.Message}");
            }
        }

        public void Send(Packet packet)
        {
            try
            {
                string json = JsonConvert.SerializeObject(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);

                lock (stream)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to send data to server: {e.Message}");
            }
        }

        private void OnDataReceived(IAsyncResult result)
        {
            try
            {
                int bytesRead = stream.EndRead(result);
                string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Packet packet = JsonConvert.DeserializeObject<Packet>(json);

                _moduleManager.GetModule(packet.TargetModule).RecievePacket(packet);

                stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to receive data from server: {e.Message}");
            }
        }
    }
}
