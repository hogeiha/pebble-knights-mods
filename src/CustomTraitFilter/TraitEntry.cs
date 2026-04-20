using System.Reflection;

namespace CustomTraitFilter
{
    internal sealed class TraitEntry
    {
        public string Id;
        public string DisplayName;
        public string NameKey;
        public string DescriptionKey;
        public string Category;
        public bool Enabled;
        public bool OriginalAvailability;
        public object SourceObject;
        public FieldInfo AvailabilityField;

        public void Apply()
        {
            if (SourceObject == null || AvailabilityField == null)
                return;

            AvailabilityField.SetValue(SourceObject, Enabled && OriginalAvailability);
        }
    }
}
