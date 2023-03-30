using BS_BotBridge_Shared.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BS_BotBridge_Core
{
    public class Client
    {
        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly byte[] buffer;
        private readonly IPA.Logging.Logger Logger;

        public Dictionary<string, Action<object>> _moduleCallbacks = new Dictionary<string, Action<object>>();

        public Client(string address, int port, IPA.Logging.Logger logger)
        {
            Logger = logger;
            try
            {
                client = new TcpClient(address, port);
                stream = client.GetStream();
                buffer = new byte[client.ReceiveBufferSize];
                stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
            }
            catch (SocketException e)
            {
                Logger.Error($"Failed to connect to server: {e.Message}");
            }
        }

        public void Send(Packet packet)
        {
            try
            {
                string json = JsonConvert.SerializeObject(packet);
                byte[] data = Encoding.UTF8.GetBytes(json);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to send data to server: {e.Message}");
            }
        }

        private void OnDataReceived(IAsyncResult result)
        {
            try
            {
                int bytesRead = stream.EndRead(result);
                string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Packet packet = JsonConvert.DeserializeObject<Packet>(json);

                foreach (var item in _moduleCallbacks)
                {

                }

                stream.BeginRead(buffer, 0, buffer.Length, OnDataReceived, null);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to receive data from server: {e.Message}");
            }
        }
    }
}
