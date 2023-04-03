using System;

namespace BSBBLib
{
    [Serializable]
    public class Packet
    {
        public string TargetModule { get; private set; }
        public int PacketType { get; private set; }
        public byte[] Data { get; private set; }

        /// <summary>
        /// Creates a new packet
        /// </summary>
        /// <param name="targetModule">The identification string for the target module (case insensitive)</param>
        /// <param name="data">Serialized packet data</param>
        /// <param name="packetType">Packet type number (for enum parsing)</param>
        public Packet(string targetModule, byte[] data, int packetType = 0)
        {
            TargetModule = targetModule;
            Data = data;
            PacketType = packetType;
        }
    }
}
