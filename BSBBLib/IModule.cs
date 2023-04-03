using HMUI;
using System;
using System.Collections.Generic;

namespace BSBBLib
{
    public interface IModule
    {
        /// <summary>
        /// Your module inicialization here
        /// </summary>
        /// <param name="sendPacket">Callback to the client for sending data</param>
        void Initialize(Action<Packet> sendPacket);

        /// <summary>
        /// This is called when data for your module is recieved
        /// </summary>
        /// <param name="packet">Parsed packet for your module to handle</param>
        void RecievePacket(Packet packet);

        /// <summary>
        /// This method should return your settings FlowCoordinator instance. Return null if not used.
        /// </summary>
        /// <returns>Module settings FlowCoordinator or null if none</returns>
        FlowCoordinator GetModuleFlowCoordinator();

        /// <summary>
        /// This should return a display friendly name for your module, to display on the button
        /// </summary>
        /// <returns>Display friendly name of the module</returns>
        string GetDisplayName();

        /// <summary>
        /// This should return your button hover text. Return null if not used.
        /// </summary>
        /// <returns>Modules button hover text</returns>
        string GetHoverText();
    }
}
