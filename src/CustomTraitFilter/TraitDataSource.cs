using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CustomTraitFilter
{
    internal static class TraitDataSource
    {
        private static readonly Dictionary<string, bool> BaselineAvailability = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public static bool IsDatabaseLoaded()
        {
            var databaseType = FindType("KBluePurple.Data.Database");
            if (databaseType == null)
                return false;

            var property = databaseType.GetProperty("IsLoaded", BindingFlags.Public | BindingFlags.Static);
            if (property == null)
                return false;

            try
            {
                return (bool)property.GetValue(null, null);
            }
            catch
            {
                return false;
            }
        }

        public static List<TraitEntry> LoadTraits()
        {
            var result = new List<TraitEntry>();
            var databaseType = FindType("KBluePurple.Data.Database");
            var traitType = FindType("Data.Traits.Trait");
            if (databaseType == null || traitType == null)
                return result;

            var getAll = FindGenericGetAll(databaseType);
            if (getAll == null)
                return result;

            var idField = traitType.GetField("Id", BindingFlags.Public | BindingFlags.Instance);
            var nameField = traitType.GetField("Name", BindingFlags.Public | BindingFlags.Instance);
            var nameIdField = traitType.GetField("NameId", BindingFlags.Public | BindingFlags.Instance);
            var descriptionIdField = traitType.GetField("DescriptionId", BindingFlags.Public | BindingFlags.Instance);
            var rankField = traitType.GetField("Rank", BindingFlags.Public | BindingFlags.Instance);
            var starField = traitType.GetField("StarCount", BindingFlags.Public | BindingFlags.Instance);
            var availabilityField = traitType.GetField("IsEnabled", BindingFlags.Public | BindingFlags.Instance);

            if (idField == null || availabilityField == null)
                return result;

            IEnumerable rows;
            try
            {
                rows = getAll.MakeGenericMethod(traitType).Invoke(null, null) as IEnumerable;
            }
            catch
            {
                return result;
            }

            if (rows == null)
                return result;

            foreach (var row in rows)
            {
                var id = ReadString(row, idField);
                if (string.IsNullOrEmpty(id))
                    continue;

                var currentAvailability = ReadBool(row, availabilityField, true);
                bool originalAvailability;
                if (!BaselineAvailability.TryGetValue(id, out originalAvailability))
                {
                    originalAvailability = currentAvailability;
                    BaselineAvailability[id] = originalAvailability;
                }

                if (!originalAvailability)
                    continue;

                var entry = new TraitEntry();
                entry.Id = id;
                entry.NameKey = ReadString(row, nameIdField);
                entry.DescriptionKey = ReadString(row, descriptionIdField);
                entry.DisplayName = FirstNonEmpty(ReadString(row, nameField), entry.NameKey, id);
                entry.Category = BuildCategory(row, rankField, starField);
                entry.Enabled = true;
                entry.OriginalAvailability = originalAvailability;
                entry.SourceObject = row;
                entry.AvailabilityField = availabilityField;
                result.Add(entry);
            }

            result.Sort(delegate(TraitEntry left, TraitEntry right)
            {
                return string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
            });

            return result;
        }

        private static MethodInfo FindGenericGetAll(Type databaseType)
        {
            var methods = databaseType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name == "GetAll" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
                    return method;
            }

            return null;
        }

        private static Type FindType(string fullName)
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

        private static string ReadString(object source, FieldInfo field)
        {
            if (source == null || field == null)
                return "";

            var value = field.GetValue(source);
            return value == null ? "" : value.ToString();
        }

        private static bool ReadBool(object source, FieldInfo field, bool fallback)
        {
            if (source == null || field == null)
                return fallback;

            var value = field.GetValue(source);
            if (value is bool)
                return (bool)value;

            return fallback;
        }

        private static string BuildCategory(object source, FieldInfo rankField, FieldInfo starField)
        {
            var rank = ReadString(source, rankField);
            var star = ReadString(source, starField);

            if (!string.IsNullOrEmpty(rank) && !string.IsNullOrEmpty(star))
                return rank + " / " + star + " star";
            if (!string.IsNullOrEmpty(rank))
                return rank;
            if (!string.IsNullOrEmpty(star))
                return star + " star";

            return "";
        }

        private static string FirstNonEmpty(string a, string b, string c)
        {
            if (!string.IsNullOrEmpty(a))
                return a;
            if (!string.IsNullOrEmpty(b))
                return b;
            return c;
        }
    }
}
