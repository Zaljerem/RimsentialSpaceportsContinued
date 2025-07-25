using Verse;
using Verse.AI.Group;

namespace Spaceports.Triggers;

public class Trigger_PawnCannotReachMapEdgeExceptOrbitExceptOrbit: Trigger
{
    public override bool ActivateOn(Lord lord, TriggerSignal signal)
    {
        if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % 197 == 0)
        {
            foreach (var ownedPawn in lord.ownedPawns)
            {
                if ((ownedPawn.Map is null || ownedPawn.Map.TileInfo.OnSurface) && ownedPawn.Spawned && !ownedPawn.Dead && !ownedPawn.Downed && !ownedPawn.CanReachMapEdge())
                    return true;
            }
        }
        return false;
    }
}