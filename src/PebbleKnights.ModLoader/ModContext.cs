using BepInEx.Logging;
using UnityEngine;

namespace PebbleKnights.ModLoader
{
    internal sealed class ModContext : IModContext
    {
        public string GameRootPath { get; private set; }
        public string ModsRootPath { get; private set; }
        public string ModDirectory { get; private set; }
        public ModManifest Manifest { get; private set; }
        public ManualLogSource Logger { get; private set; }

        public ModContext(string gameRootPath, string modsRootPath, string modDirectory, ModManifest manifest, ManualLogSource logger)
        {
            GameRootPath = gameRootPath;
            ModsRootPath = modsRootPath;
            ModDirectory = modDirectory;
            Manifest = manifest;
            Logger = logger;
        }

        public T AddBehaviour<T>(string objectName) where T : MonoBehaviour
        {
            var gameObject = new GameObject(objectName);
            Object.DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<T>();
        }
    }
}
