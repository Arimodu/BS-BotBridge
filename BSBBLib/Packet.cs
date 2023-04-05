using System;

namespace BSBBLib
{
    // Not exactly sure where to put this
    // So now its here, out of the way kinda
    public enum ConnectionState
    {
        Connecting,
        Connected,
        Disabled,
        Errored
    }

    public enum PacketType
    {
        Heartbeat,
        CoreData,
        ModuleData
    }

    /// <summary>
    /// A basic packet structure
    /// </summary>
    [Serializable]
    public class Packet
    {
        /// <summary>
        /// Target module identifier
        /// </summary>
        public string TargetModule { get; private set; }

        public PacketType CorePacketType { get; private set; }
        /// <summary>
        /// This is designed for enum parsing
        /// </summary>
        public int ModulePacketType { get; private set; }

        /// <summary>
        /// Serialized packet data
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Creates a new packet
        /// </summary>
        /// <param name="targetModule">The identification string for the target module (case insensitive)</param>
        /// <param name="data">Serialized packet data</param>
        /// <param name="packetType">Packet type number (for enum parsing)</param>
        /// <param name="type">Core Packet type. Leave this default, this is used for Heartbeat and CoreData, under all circumstances you should use ModuleData inside your own module</param>
        public Packet(string targetModule, byte[] data, int packetType = 0, PacketType type = PacketType.ModuleData)
        {
            TargetModule = targetModule;
            Data = data;
            ModulePacketType = packetType;
            CorePacketType = type;
        }
    }
}
