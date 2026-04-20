using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace PebbleKnights.ModLoader
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ModLoaderPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.pebbleknights.modloader";
        public const string PluginName = "Pebble Knights Mod Loader";
        public const string PluginVersion = "0.1.0";

        private readonly List<string> _assemblySearchDirectories = new List<string>();
        private readonly List<LoadedMod> _loadedMods = new List<LoadedMod>();
        private string _gameRootPath;
        private string _modsRootPath;

        public static ModLoaderPlugin Instance { get; private set; }
        public static IReadOnlyList<LoadedMod> LoadedMods
        {
            get
            {
                if (Instance == null)
                    return new LoadedMod[0];

                return Instance._loadedMods.AsReadOnly();
            }
        }

        private void Awake()
        {
            Instance = this;
            _gameRootPath = Paths.GameRootPath;
            _modsRootPath = Path.Combine(_gameRootPath, "Mods");

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            Directory.CreateDirectory(_modsRootPath);
            AddAssemblyDirectory(Path.Combine(_gameRootPath, "Pebble Knights_Data", "Managed"));
            AddAssemblyDirectory(Path.Combine(_gameRootPath, "BepInEx", "core"));

            Logger.LogInfo("Pebble Knights Mod Loader started.");
            Logger.LogInfo("Mods folder: " + _modsRootPath);
            LoadAllMods();
        }

        private void LoadAllMods()
        {
            var directories = Directory.GetDirectories(_modsRootPath);
            Array.Sort(directories, StringComparer.OrdinalIgnoreCase);

            if (directories.Length == 0)
            {
                Logger.LogInfo("No mod folders found.");
                return;
            }

            for (var i = 0; i < directories.Length; i++)
            {
                LoadModDirectory(directories[i]);
            }

            Logger.LogInfo("Loaded " + _loadedMods.Count + " mod(s).");
        }

        private void LoadModDirectory(string modDirectory)
        {
            var manifestPath = Path.Combine(modDirectory, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                Logger.LogWarning("Skipping folder without manifest.json: " + modDirectory);
                return;
            }

            ModManifest manifest;
            try
            {
                manifest = ManifestParser.Parse(File.ReadAllText(manifestPath), Path.GetFileName(modDirectory));
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to read manifest: " + manifestPath);
                Logger.LogError(ex);
                return;
            }

            if (!manifest.Enabled)
            {
                Logger.LogInfo("Skipping disabled mod: " + manifest.Id);
                return;
            }

            var pluginDirectory = Path.Combine(modDirectory, "plugins");
            if (!Directory.Exists(pluginDirectory))
            {
                Logger.LogWarning("Skipping mod without plugins folder: " + manifest.Id);
                return;
            }

            AddAssemblyDirectory(pluginDirectory);

            var assemblies = new List<Assembly>();
            var dllFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            Array.Sort(dllFiles, StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < dllFiles.Length; i++)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFrom(dllFiles[i]));
                    Logger.LogInfo("Loaded mod assembly: " + dllFiles[i]);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to load mod assembly: " + dllFiles[i]);
                    Logger.LogError(ex);
                }
            }

            var entryType = FindEntryType(assemblies, manifest.Entry);
            if (entryType == null)
            {
                Logger.LogWarning("Could not find IPebbleMod entry type for mod: " + manifest.Id);
                return;
            }

            try
            {
                var modLogger = BepInEx.Logging.Logger.CreateLogSource("Mod:" + manifest.Id);
                var context = new ModContext(_gameRootPath, _modsRootPath, modDirectory, manifest, modLogger);
                var instance = (IPebbleMod)Activator.CreateInstance(entryType);
                instance.Load(context);
                _loadedMods.Add(new LoadedMod(manifest, modDirectory, instance));
                Logger.LogInfo("Started mod: " + manifest.Name + " " + manifest.Version);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to start mod: " + manifest.Id);
                Logger.LogError(ex);
            }
        }

        private Type FindEntryType(List<Assembly> assemblies, string entryTypeName)
        {
            if (!string.IsNullOrEmpty(entryTypeName))
            {
                for (var i = 0; i < assemblies.Count; i++)
                {
                    var directType = assemblies[i].GetType(entryTypeName, false);
                    if (directType != null && typeof(IPebbleMod).IsAssignableFrom(directType))
                        return directType;
                }
            }

            for (var i = 0; i < assemblies.Count; i++)
            {
                var types = SafeGetTypes(assemblies[i]);
                for (var j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    if (type == null || type.IsAbstract || !typeof(IPebbleMod).IsAssignableFrom(type))
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) != null)
                        return type;
                }
            }

            return null;
        }

        private Type[] SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types;
            }
        }

        private void AddAssemblyDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            for (var i = 0; i < _assemblySearchDirectories.Count; i++)
            {
                if (string.Equals(_assemblySearchDirectories[i], directory, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            _assemblySearchDirectories.Add(directory);
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var requestedName = new AssemblyName(args.Name).Name;
            var loaded = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < loaded.Length; i++)
            {
                if (string.Equals(loaded[i].GetName().Name, requestedName, StringComparison.OrdinalIgnoreCase))
                    return loaded[i];
            }

            for (var i = 0; i < _assemblySearchDirectories.Count; i++)
            {
                var candidate = Path.Combine(_assemblySearchDirectories[i], requestedName + ".dll");
                if (!File.Exists(candidate))
                    continue;

                try
                {
                    return Assembly.LoadFrom(candidate);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}
