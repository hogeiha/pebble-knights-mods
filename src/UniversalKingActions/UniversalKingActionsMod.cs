using PebbleKnights.ModLoader;

namespace UniversalKingActions
{
    public sealed class UniversalKingActionsMod : IPebbleMod
    {
        public void Load(IModContext context)
        {
            var behaviour = context.AddBehaviour<UniversalKingActionsBehaviour>("Universal King Actions");
            behaviour.Initialize(context);
        }
    }
}
