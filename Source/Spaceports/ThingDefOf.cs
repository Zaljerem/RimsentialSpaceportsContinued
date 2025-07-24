using RimWorld;
using Verse;

namespace Spaceports
{
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef Spaceports_ShuttleLandingPad;
        public static ThingDef Silver;
        public static ThingDef Gold;
        public static ThingDef ChunkSlagSteel;
        public static ThingDef PowerConduit;

        static ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }
    }
}