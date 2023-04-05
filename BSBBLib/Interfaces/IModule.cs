using HMUI;
using System;
using System.Collections.Generic;

namespace BSBBLib.Interfaces
{
    public interface IModule
    {
        FlowCoordinator FlowCoordinator { get; }
        string DisplayName { get; }
        string HoverText { get; }

        /// <summary>
        /// Your module inicialization here
        /// </summary>
        /// <param name="client">Core client interface, use this to interact with Core client instance</param>
        void Initialize(IClient client);

        /// <summary>
        /// This is called when data for your module is recieved
        /// </summary>
        /// <param name="packet">Parsed packet for your module to handle</param>
        void RecievePacket(Packet packet);
    }
}
