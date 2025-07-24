using RimWorld;
using Verse;
using Verse.AI;


namespace Spaceports
{

    public class Verb_CastCallShuttle : Verb_CastBase
    {
        protected override bool TryCastShot()
        {
            Pawn casterPawn = this.CasterPawn;
            //Thing thing = this.currentTarget.Thing;
            //if (casterPawn == null || thing == null)
            if (casterPawn == null)
            {
                return false;
            }
            else
            {
                //Thing thing = ThingMaker.MakeThing(SpaceportsDefOf.Spaceports_RoyaltyShuttle);
                Thing thing = ThingMaker.MakeThing(SpaceportsDefOf.Shuttle);
                CompShuttle compShuttle = thing.TryGetComp<CompShuttle>();
                compShuttle.permitShuttle = true;
                compShuttle.acceptChildren = true;
                //TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Spaceports_RoyaltyShuttle, null, thing);
                TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, null, thing);
                Map currentMap = Find.CurrentMap;
                transportShip.ArriveAt(DropCellFinder.GetBestShuttleLandingSpot(currentMap, Faction.OfPlayer), currentMap.Parent);
                transportShip.AddJobs(ShipJobDefOf.WaitForever, ShipJobDefOf.Unload_Destination, ShipJobDefOf.FlyAway);
                this.ReloadableCompSource?.UsedOnce();
                return true;
            }

        }
    }
}
