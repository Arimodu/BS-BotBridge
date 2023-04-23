using System;
using System.Net;

namespace BSBBCore
{
    public enum ConnectionState
    {
        Disconnected,
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

    public static class SharedValues
    {
        public static IPAddress Address => IPAddress.Loopback;
        public static int Port => 7227;
        public static int BufferLength => 4096;
        public static TimeSpan HeartbeatInterval => TimeSpan.FromMilliseconds(5000);
        public static TimeSpan HeartbeatTimeout => TimeSpan.FromMilliseconds(10000);
    }
}
