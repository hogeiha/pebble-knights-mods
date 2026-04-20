using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomTraitFilter
{
    internal sealed class TraitFilterConfig
    {
        public readonly HashSet<string> DisabledTraits = new HashSet<string>();
    }

    internal static class ConfigStore
    {
        public static TraitFilterConfig Load(string path)
        {
            var config = new TraitFilterConfig();
            if (!File.Exists(path))
                return config;

            var json = File.ReadAllText(path);
            ReadArray(json, "disabledTraits", config.DisabledTraits);
            return config;
        }

        public static void Save(string path, IEnumerable<string> disabledTraits)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.Append("  \"disabledTraits\": ");
            WriteArray(builder, disabledTraits);
            builder.AppendLine();
            builder.AppendLine("}");
            File.WriteAllText(path, builder.ToString());
        }

        private static void ReadArray(string json, string key, HashSet<string> target)
        {
            var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\\[(.*?)\\]";
            var match = Regex.Match(json, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!match.Success)
                return;

            var itemMatches = Regex.Matches(match.Groups[1].Value, "\"((?:\\\\.|[^\"])*)\"");
            for (var i = 0; i < itemMatches.Count; i++)
                target.Add(Unescape(itemMatches[i].Groups[1].Value));
        }

        private static void WriteArray(StringBuilder builder, IEnumerable<string> values)
        {
            builder.Append("[");
            var first = true;
            foreach (var value in values)
            {
                if (!first)
                    builder.Append(", ");

                builder.Append("\"");
                builder.Append(Escape(value));
                builder.Append("\"");
                first = false;
            }
            builder.Append("]");
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string Unescape(string value)
        {
            return value.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }
}
