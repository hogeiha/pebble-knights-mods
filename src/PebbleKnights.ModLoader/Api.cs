using System;
using BepInEx.Logging;
using UnityEngine;

namespace PebbleKnights.ModLoader
{
    public interface IPebbleMod
    {
        void Load(IModContext context);
    }

    public interface IModContext
    {
        string GameRootPath { get; }
        string ModsRootPath { get; }
        string ModDirectory { get; }
        ModManifest Manifest { get; }
        ManualLogSource Logger { get; }

        T AddBehaviour<T>(string objectName) where T : MonoBehaviour;
    }

    public sealed class ModManifest
    {
        public string Id;
        public string Name;
        public string Version;
        public string Author;
        public string Entry;
        public string Description;
        public bool Enabled;
        public string[] Tags;

        public ModManifest()
        {
            Id = "";
            Name = "";
            Version = "0.0.0";
            Author = "";
            Entry = "";
            Description = "";
            Enabled = true;
            Tags = new string[0];
        }
    }

    public sealed class LoadedMod
    {
        public ModManifest Manifest;
        public string Directory;
        public IPebbleMod Instance;

        public LoadedMod(ModManifest manifest, string directory, IPebbleMod instance)
        {
            Manifest = manifest;
            Directory = directory;
            Instance = instance;
        }
    }
}
