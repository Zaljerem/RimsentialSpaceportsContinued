using RimWorld;
using Verse;

namespace Spaceports
{
    [DefOf]
    public static class TransportShipDefOf
    {
        public static TransportShipDef Spaceports_RoyaltyShuttle;
        public static TransportShipDef Spaceports_ShuttleA;
        public static TransportShipDef Spaceports_ShuttleB;
        public static TransportShipDef Spaceports_ShuttleC;
        public static TransportShipDef Spaceports_ShuttleD;
        public static TransportShipDef Spaceports_ShuttleE;
        public static TransportShipDef Spaceports_ShuttleF;
        public static TransportShipDef Spaceports_ShuttleG;
        public static TransportShipDef Spaceports_ShuttleH;
        public static TransportShipDef Spaceports_ShuttleI;
        public static TransportShipDef Spaceports_ShuttleJ;
        public static TransportShipDef Spaceports_ShuttleK;
        public static TransportShipDef Spaceports_ShuttleSkip;
        public static TransportShipDef Spaceports_ShuttleInert;
        public static TransportShipDef Spaceports_SurpriseShuttle;
        public static TransportShipDef Ship_Shuttle;

        static TransportShipDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TransportShipDefOf));
        }
    }
}