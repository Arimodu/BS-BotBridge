using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SiraUtil.Logging;
using BS_BotBridge_Core.Configuration;
using BS_BotBridge_Shared;
using BS_BotBridge_Core.Managers;

namespace BS_BotBridge_Core
{
    public class Client
    {
        private readonly TcpClient client;
        private NetworkStream stream;
        private byte[] buffer;
        private readonly SiraLog logger;
        private readonly string address;
        private readonly int port;
        private BSBBModuleManager _moduleManager;

        public Client(SiraLog siraLog, PluginConfig config)
        {
            logger = siraLog;
            client = new TcpClient();
            address = config.ServerAddress;
            port = config.ServerPort;
        }

        public void Start()
        {
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

        /// <summary>
        /// Yes, this creates a circular dependency, do I care? NO. Fuck maintainability, this is my bonfire
        /// </summary>
        internal void CreateCircularDependency(BSBBModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }
    }
}
