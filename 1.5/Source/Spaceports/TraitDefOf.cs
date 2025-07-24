using RimWorld;
using Verse;

namespace Spaceports
{
    [DefOf]
    public static class TraitDefOf
    {
        public static TraitDef Tough;
        public static TraitDef ShootingAccuracy;
        public static TraitDef Abrasive;
        public static TraitDef Psychopath;
        public static TraitDef Brawler;

        static TraitDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TraitDefOf));
        }
    }
}