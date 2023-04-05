using System;

namespace BSBBLib.Interfaces
{
    public interface IClient
    {
        event Action<ConnectionState> OnStateChanged;
        ConnectionState State { get; }

        /// <summary>
        /// Call this to send a packet to the server
        /// </summary>
        /// <param name="packet">Packet to send to the server</param>
        void Send(Packet packet);
    }
}
