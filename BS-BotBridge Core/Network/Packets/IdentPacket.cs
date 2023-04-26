using System;

namespace BSBBCore.Network.Packets
{
    [Serializable]
    public class IdentPacket
    {
        /// <summary>
        /// The identifier for this module
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Some data to be passed along, not used currently
        /// </summary>
        //public byte[] Data { get; private set; } = null;

        public IdentPacket(string identifier)
        { 
            Identifier = identifier;
        }
    }
}
