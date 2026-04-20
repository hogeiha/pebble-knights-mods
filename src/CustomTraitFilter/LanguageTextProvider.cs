using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;

namespace CustomTraitFilter
{
    internal sealed class LanguageTextProvider
    {
        private readonly Dictionary<string, string> _texts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private ManualLogSource _logger;
        private string _languagesDirectory;
        private string _languageCode = "";

        public string LanguageCode { get { return _languageCode; } }

        public void Initialize(string gameRootPath, ManualLogSource logger)
        {
            _logger = logger;
            _languagesDirectory = Path.Combine(gameRootPath, "Pebble Knights_Data", "Languages");
            RefreshIfNeeded(true);
        }

        public void RefreshIfNeeded(bool force)
        {
            var code = GetCurrentLanguageCode();
            if (string.IsNullOrEmpty(code))
                code = "zh_CN";

            if (!force && string.Equals(code, _languageCode, StringComparison.OrdinalIgnoreCase))
                return;

            LoadLanguageFile(code);
        }

        public string TraitName(TraitEntry entry)
        {
            if (entry == null)
                return "";

            if (!string.IsNullOrEmpty(entry.NameKey))
                return TextOrFallback(entry.NameKey, entry.DisplayName);

            return entry.DisplayName;
        }

        public string TextOrFallback(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key))
                return fallback;

            string value;
            if (_texts.TryGetValue(key, out value))
                return StripRichText(value);

            return fallback;
        }

        public string Ui(string key)
        {
            var isChinese = _languageCode.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
            if (!isChinese)
                return UiEnglish(key);

            switch (key)
            {
                case "WindowTitle": return "特性自定义";
                case "Search": return "搜索";
                case "Refresh": return "刷新";
                case "Apply": return "应用";
                case "EnableAll": return "全部启用";
                case "DisableAll": return "全部禁用";
                case "Enabled": return "启用";
                case "Disabled": return "禁用";
                case "Applied": return "已应用";
                case "Hotkeys": return "F8 关闭   F9 刷新";
                case "NoEntries": return "还没有读取到特性。进入主菜单或准备房间后按 F9。";
                case "StatusWaiting": return "正在等待游戏数据表...";
                default: return key;
            }
        }

        private string UiEnglish(string key)
        {
            switch (key)
            {
                case "WindowTitle": return "Custom Trait Filter";
                case "Search": return "Search";
                case "Refresh": return "Refresh";
                case "Apply": return "Apply";
                case "EnableAll": return "Enable All";
                case "DisableAll": return "Disable All";
                case "Enabled": return "ON";
                case "Disabled": return "OFF";
                case "Applied": return "Applied";
                case "Hotkeys": return "F8 Close   F9 Refresh";
                case "NoEntries": return "No traits loaded yet. Enter the main menu or ready room, then press F9.";
                case "StatusWaiting": return "Waiting for game database...";
                default: return key;
            }
        }

        private void LoadLanguageFile(string code)
        {
            var path = Path.Combine(_languagesDirectory, code + ".json");
            if (!File.Exists(path))
            {
                path = Path.Combine(_languagesDirectory, "zh_CN.json");
                code = "zh_CN";
            }
            if (!File.Exists(path))
            {
                path = Path.Combine(_languagesDirectory, "en_US.json");
                code = "en_US";
            }

            _texts.Clear();
            _languageCode = code;

            if (!File.Exists(path))
                return;

            var json = File.ReadAllText(path);
            var matches = Regex.Matches(json, "\"((?:\\\\.|[^\"])*)\"\\s*:\\s*\"((?:\\\\.|[^\"])*)\"", RegexOptions.Singleline);
            for (var i = 0; i < matches.Count; i++)
                _texts[Unescape(matches[i].Groups[1].Value)] = Unescape(matches[i].Groups[2].Value);

            if (_logger != null)
                _logger.LogInfo("Loaded language file: " + path + " (" + _texts.Count + " entries)");
        }

        private string GetCurrentLanguageCode()
        {
            try
            {
                var languageManager = FindType("LanguageManager");
                if (languageManager == null)
                    return "";

                var property = languageManager.GetProperty("Language", BindingFlags.Public | BindingFlags.Static);
                if (property == null)
                    return "";

                var value = property.GetValue(null, null);
                return value == null ? "" : value.ToString();
            }
            catch
            {
                return "";
            }
        }

        private Type FindType(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType(fullName, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static string StripRichText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return Regex.Replace(value, "<.*?>", "");
        }

        private static string Unescape(string value)
        {
            return value
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }
    }
}
