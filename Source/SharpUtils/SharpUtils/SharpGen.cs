using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SharpUtils;

public class SharpGen
{
	public static Thing GenThing(ThingDef thing, int amount = 1)
	{
		Thing thing2 = ThingMaker.MakeThing(thing);
		thing2.stackCount = amount;
		return thing2;
	}

	public static Thing GenThing(ThingDef thing, int min, int max)
	{
		Thing thing2 = ThingMaker.MakeThing(thing);
		thing2.stackCount = Rand.RangeInclusive(min, max);
		return thing2;
	}

	public static List<Thing> GenReward(int minVal, int maxVal)
	{
		ThingSetMakerParams parms = default(ThingSetMakerParams);
		parms.qualityGenerator = QualityGenerator.Reward;
		parms.totalMarketValueRange = new FloatRange(minVal, maxVal);
		return ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms);
	}

	public static List<Thing> GenTempleReward()
	{
		ThingSetMakerParams parms = default(ThingSetMakerParams);
		parms.qualityGenerator = QualityGenerator.Reward;
		return ThingSetMakerDefOf.MapGen_AncientTempleContents.root.Generate(parms);
	}
}
