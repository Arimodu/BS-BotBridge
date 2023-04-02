using System;

namespace BS_BotBridge_Shared
{
    public interface IModule
    {
        void Initialize(Action<Packet> sendPacket);
        void RecievePacket(Packet packet);
    }
}
