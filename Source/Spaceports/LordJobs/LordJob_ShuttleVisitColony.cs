using RimWorld;
using Spaceports.LordToils;
using System.Collections.Generic;
using System.Linq;
using Spaceports.Triggers;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Spaceports.LordJobs
{
    public class LordJob_ShuttleVisitColony : LordJob
    {
        private Faction faction;

        private IntVec3 chillSpot;

        private Thing shuttle;

        private int? durationTicks;

        public StateGraph exitSubgraph;

        public LordJob_ShuttleVisitColony(Faction faction, IntVec3 chillSpot, Thing shuttle, int? durationTicks = null)
        {
            this.faction = faction;
            this.chillSpot = chillSpot;
            this.durationTicks = durationTicks;
            this.shuttle = shuttle;
        }

        public override void LordJobTick()
        {
            base.LordJobTick();
            if(Find.TickManager.TicksGame % 250 == 0) 
            { 
                Utils.VerifyRequiredPawns(this.lord, this.shuttle);
            }

            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                // Check if we're on an orbital map and the shuttle is null or destroyed
                if ((shuttle == null || shuttle.Destroyed) && !lord.Map.TileInfo.OnSurface)
                {
                    // Call for a new shuttle
                    CallForNewShuttle();
                }
            }
        }

        private void CallForNewShuttle()
        {
            if (!Utils.CheckIfClearForLanding(lord.Map, 2))

            // Log the shuttle request
            if (faction != null)
            {
                Messages.Message("Visitors from " + faction.Name + " are calling for a new shuttle to pick them up because their ship has left or was destroyed.", MessageTypeDefOf.NeutralEvent);
            }
            else
            {
                Messages.Message("Visitors are calling for a new shuttle to pick them up because their ship has left or was destroyed.", MessageTypeDefOf.NeutralEvent);
            }
            
            Map map = lord.Map;
            List<Pawn> pawns = lord.ownedPawns.ToList();
            
            IntVec3 pad = Utils.FindValidSpaceportPad(map, faction, 2);
            TransportShip ship = Utils.GenerateInboundShuttle(pawns, pad, map, rescue:true);
            shuttle = ship.shipThing;

            lord.RemoveAllPawns();
            lord.Map.lordManager.RemoveLord(lord);
            
            LordJob lordJob = new LordJob_ShuttleVisitColony(faction, Utils.GetBestChillspot(map, pad, 2), shuttle: ship.shipThing);
            lord = LordMaker.MakeNewLord(faction, lordJob, map, pawns);
        }


        public override StateGraph CreateGraph()
        {
            // Initialize the state graph for managing visitor behaviors
            StateGraph stateGraph = new StateGraph();
            
            // Create initial travel state - visitors moving to their designated spot
            LordToil lordToil = (stateGraph.StartingToil = stateGraph.AttachSubgraph(new LordJob_Travel(chillSpot).CreateGraph()).StartingToil);
            
            // Create defend point state - visitors hanging out at their spot
            LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(chillSpot);
            stateGraph.AddToil(lordToil_DefendPoint);
            
            // Create state for rescuing wounded guests
            LordToil_TryShuttleWoundedGuest lordToil_TakeWoundedGuest = new LordToil_TryShuttleWoundedGuest(shuttle, LocomotionUrgency.Sprint, canDig: false);
            stateGraph.AddToil(lordToil_TakeWoundedGuest);
            
            // Create exit states for departing visitors
            exitSubgraph = new LordJob_DepartSpaceport(shuttle).CreateGraph();
            LordToil target = exitSubgraph.lordToils[1];
            LordToil startingToil2 = new LordToil_EnterShuttleOrLeaveNullChecked(shuttle, LocomotionUrgency.Walk, canDig: true);
            stateGraph.AddToil(startingToil2);
            LordToil_EnterShuttleOrLeaveNullChecked lordToil_ExitMap = new LordToil_EnterShuttleOrLeaveNullChecked(shuttle, LocomotionUrgency.Walk, canDig: true);
            stateGraph.AddToil(lordToil_ExitMap);

            // Transition 1: Leave if temperature is dangerous
            Transition transition = new Transition(lordToil, startingToil2);
            transition.AddSources(lordToil_DefendPoint);
            transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
            if (faction != null)
            {
                transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            }
            transition.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition);

            // Transition 2: Exit map if pawns are trapped
            Transition transition2 = new Transition(lordToil, lordToil_ExitMap);
            transition2.AddSources(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
            transition2.AddSources(exitSubgraph.lordToils);
            transition2.AddTrigger(new Trigger_PawnCannotReachMapEdgeExceptOrbitExceptOrbit());
            if (faction != null)
            {
                transition2.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
            }
            stateGraph.AddTransition(transition2);

            // Transition 3: Return to shuttle if map edge becomes reachable
            Transition transition3 = new Transition(lordToil_ExitMap, startingToil2);
            transition3.AddTrigger(new Trigger_PawnCanReachMapEdge());
            transition3.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition3);

            // Transition 4: Move to defend point after arriving
            Transition transition4 = new Transition(lordToil, lordToil_DefendPoint);
            transition4.AddTrigger(new Trigger_Memo("TravelArrived"));
            stateGraph.AddTransition(transition4);

            // Transition 5: Help wounded guests if present
            if (faction != null)
            {
                Transition transition5 = new Transition(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
                transition5.AddTrigger(new Trigger_WoundedGuestPresent());
                transition5.AddPreAction(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
                stateGraph.AddTransition(transition5);
            }

            // Transition 6: Leave if become enemies
            Transition transition6 = new Transition(lordToil_DefendPoint, target);
            transition6.AddSources(lordToil_TakeWoundedGuest, lordToil);
            transition6.AddTrigger(new Trigger_BecamePlayerEnemy());
            transition6.AddPreAction(new TransitionAction_SetDefendLocalGroup());
            transition6.AddPostAction(new TransitionAction_WakeAll());
            transition6.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition6);

            // Transition 7: Leave after time expires
            Transition transition7 = new Transition(lordToil_DefendPoint, startingToil2);
            int tickLimit = ((!DebugSettings.instantVisitorsGift || faction == null) ? ((!durationTicks.HasValue) ? Rand.Range(6000, (int)LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorMaxTime * GenDate.TicksPerDay) : durationTicks.Value) : 0);
            transition7.AddTrigger(new Trigger_TicksPassed(tickLimit));
            if (faction != null && LoadedModManager.GetMod<SpaceportsMod>().GetSettings<SpaceportsSettings>().visitorNotifications)
            {
                transition7.AddPreAction(new TransitionAction_Message("Spaceports_VisitorsLeaving".Translate(faction.Name)));
            }
            transition7.AddPostAction(new TransitionAction_WakeAll());
            stateGraph.AddTransition(transition7);
            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref chillSpot, "chillSpot");
            Scribe_Values.Look(ref durationTicks, "durationTicks");
            Scribe_References.Look(ref shuttle, "shuttle");
        }
    }
}
