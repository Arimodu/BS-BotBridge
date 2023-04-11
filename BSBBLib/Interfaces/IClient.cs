using System;
using BSBBLib.Packets;

namespace BSBBLib.Interfaces
{
    public interface IClient
    {
        event Action<ConnectionState> OnStateChanged;
        ConnectionState State { get; }

        /// <summary>
        /// Call this to send a <see cref="Packet"/> to the server
        /// </summary>
        /// <param name="packet"><see cref="Packet"/> to send to the server</param>
        void Send(Packet packet);

        /// <summary>
        /// Call this to request a reconnect. Note that this does nothing when already connected or when in <see cref="ConnectionState.Disabled"/>
        /// </summary>
        void Reconnect();
    }
}
