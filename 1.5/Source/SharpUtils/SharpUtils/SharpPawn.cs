using System;
using Verse;

namespace SharpUtils;

public class SharpPawn
{
	public static void StripPawn(Pawn pawn)
	{
		if (pawn.inventory != null)
		{
			pawn.inventory.DestroyAll();
		}
		if (pawn.apparel != null)
		{
			pawn.apparel.DestroyAll();
		}
		if (pawn.equipment != null)
		{
			pawn.equipment.DestroyAllEquipment();
		}
	}

	public static void WipePawn(Pawn pawn)
	{
		throw new NotImplementedException();
	}
}
