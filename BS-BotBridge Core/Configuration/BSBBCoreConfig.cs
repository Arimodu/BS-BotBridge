﻿using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BSBBCore.Configuration
{
    public class BSBBCoreConfig
    {
        public static BSBBCoreConfig Instance { get; set; }
        public virtual bool ConnectionEnabled { get; set; } = false;
        public virtual string ServerAddress { get; set; } = "127.0.0.1";
        public virtual int ServerPort { get; set; } = 7227;

        public event Action OnChanged;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(BSBBCoreConfig other)
        {
            ConnectionEnabled = other.ConnectionEnabled;
            ServerAddress = other.ServerAddress;
            ServerPort = other.ServerPort;
        }
    }
}
