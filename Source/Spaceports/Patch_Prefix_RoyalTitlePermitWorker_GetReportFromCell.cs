using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace Spaceports;

// allow player shuttles to land on shuttle landing pads
[HarmonyPatch(typeof(RoyalTitlePermitWorker_CallShuttle), "GetReportFromCell")]
public static class Patch_RoyalTitlePermitWorker_GetReportFromCell
{
    public static bool Prefix(IntVec3 cell, Map map, bool interactionSpot, ThingDef shuttleDef, ref string __result)
    {
        if (!cell.InBounds(map) || cell.Fogged(map) || !cell.Walkable(map))
        {
            return true; // let vanilla handle it
        }

        List<Thing> thingList = cell.GetThingList(map);
        for (int i = 0; i < thingList.Count; i++)
        {
            Thing thing = thingList[i];

            // If a blocking building is found, check if it's our landing pad
            if (thing is IActiveTransporter || thing is Skyfaller || 
                (thing.def.category == ThingCategory.Building && !thing.def.building.isPowerConduit))
            {
                if (thing.def.defName == "Spaceports_ShuttleLandingPad")
                {
                    __result = null;
                    return false; // Skip original method
                }

                return true; // Let vanilla report the error
            }

            // Also check for items if not an interaction spot
            if (!interactionSpot && thing.def.category == ThingCategory.Item)
            {
                return true;
            }

            // Block if a tree
            if (thing.def.plant?.IsTree == true)
            {
                return true;
            }
        }

        return true; // Let vanilla handle all other cases
    }
}
