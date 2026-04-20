using System;
using System.Text.RegularExpressions;

namespace PebbleKnights.ModLoader
{
    internal static class ManifestParser
    {
        public static ModManifest Parse(string json, string fallbackId)
        {
            var manifest = new ModManifest();
            manifest.Id = ReadString(json, "id", fallbackId);
            manifest.Name = ReadString(json, "name", manifest.Id);
            manifest.Version = ReadString(json, "version", "0.0.0");
            manifest.Author = ReadString(json, "author", "");
            manifest.Entry = ReadString(json, "entry", "");
            manifest.Description = ReadString(json, "description", "");
            manifest.Enabled = ReadBool(json, "enabled", true);
            manifest.Tags = ReadStringArray(json, "tags");
            return manifest;
        }

        private static string ReadString(string json, string key, string fallback)
        {
            var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"])*)\"";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                return fallback;

            return Unescape(match.Groups[1].Value);
        }

        private static bool ReadBool(string json, string key, bool fallback)
        {
            var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*(true|false)";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
                return fallback;

            return string.Equals(match.Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] ReadStringArray(string json, string key)
        {
            var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\\[(.*?)\\]";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!match.Success)
                return new string[0];

            var itemMatches = Regex.Matches(match.Groups[1].Value, "\"((?:\\\\.|[^\"])*)\"");
            var result = new string[itemMatches.Count];
            for (var i = 0; i < itemMatches.Count; i++)
                result[i] = Unescape(itemMatches[i].Groups[1].Value);

            return result;
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
