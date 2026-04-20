using PebbleKnights.ModLoader;

namespace CustomTraitFilter
{
    public sealed class CustomTraitFilterMod : IPebbleMod
    {
        public void Load(IModContext context)
        {
            var behaviour = context.AddBehaviour<CustomTraitFilterBehaviour>("PebbleKnights.CustomTraitFilter");
            behaviour.Initialize(context);
            context.Logger.LogInfo("Custom Trait Filter loaded. Press F8 in game to open the trait filter panel.");
        }
    }
}
