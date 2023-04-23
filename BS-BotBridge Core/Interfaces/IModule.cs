using HMUI;
using BSBBCore.Network.Packets;

namespace BSBBCore.Interfaces
{
    public interface IModule
    {
        /// <summary>
        /// Front view controller. Mandatory if you want to have a button in the settings menu
        /// </summary>
        ViewController ViewController { get; }
        /// <summary>
        /// Left view contrller. Set null if not used
        /// </summary>
        ViewController LeftViewController { get; }
        /// <summary>
        /// Right view contrller. Set null if not used
        /// </summary>
        ViewController RightViewController { get; }
        /// <summary>
        /// Top view controller. Set null if not used
        /// </summary>
        ViewController TopViewController { get; }
        /// <summary>
        /// Bottom view controller. Set null if not used
        /// </summary>
        ViewController BottomViewController { get; }

        /// <summary>
        /// Your modules display name. Will be used on your menu button
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Your hover text. Will be displayed on hover over your button
        /// </summary>
        string HoverText { get; }

        /// <summary>
        /// Your module inicialization here
        /// </summary>
        /// <param name="client">Core client interface, use this to interact with Core client instance</param>
        void Initialize(IClient client);

        /// <summary>
        /// This is called when data for your module is recieved
        /// </summary>
        /// <param name="packet">Parsed <see cref="Packet"/> for your module to handle</param>
        void RecievePacket(Packet packet);
    }
}
