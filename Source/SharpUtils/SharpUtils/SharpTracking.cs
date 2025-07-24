using System.Collections.Generic;
using Verse;

namespace SharpUtils;

public class SharpTracking
{
	public abstract class Tracker : IExposable
	{
		private int TrackerTimeout;

		private int TicksPassed = 0;

		private string TrackerIdent;

		public Tracker(int TrackerTimeout = -1, string TrackerIdent = "N/A")
		{
			this.TrackerTimeout = TrackerTimeout;
			this.TrackerIdent = TrackerIdent;
		}

		public abstract bool Check();

		public bool CheckTimeout()
		{
			if (TrackerTimeout < 0)
			{
				return false;
			}
			if (TicksPassed >= TrackerTimeout)
			{
				return true;
			}
			TicksPassed++;
			return false;
		}

		public abstract void ExposeData();

		public string GetTrackerInfo()
		{
			return "[SharpUtils] Tracker ID: " + TrackerIdent + "; Timeout set to: " + TrackerTimeout + " ticks, " + TicksPassed + " ticks have passed since instantiation.";
		}
	}

	public abstract class TrackerComponent : MapComponent
	{
		private List<Tracker> trackers = new List<Tracker>();

		public TrackerComponent(Map map)
			: base(map)
		{
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref trackers, "trackers", LookMode.Deep);
			base.ExposeData();
		}

		public override void MapComponentTick()
		{
			base.MapComponentTick();
			if (trackers == null)
			{
				trackers = new List<Tracker>();
			}
			List<Tracker> list = new List<Tracker>();
			foreach (Tracker tracker in trackers)
			{
				if (tracker.Check())
				{
					list.Add(tracker);
				}
				if (tracker.CheckTimeout())
				{
					list.Add(tracker);
				}
			}
			foreach (Tracker item in list)
			{
				trackers.Remove(item);
			}
		}

		public void DumpTrackerInfo()
		{
			Log.Message("[SharpUtils] Performing tracker info dump!");
			foreach (Tracker tracker in trackers)
			{
				Log.Message(tracker.GetTrackerInfo());
			}
		}

		public void LoadTracker(Tracker tracker)
		{
			trackers.Add(tracker);
		}
	}

	public static MapComponent GetTrackerComponent(Map map)
	{
		return map.GetComponent<TrackerComponent>();
	}

	public static void LoadTrackerIntoComponent(Map map, Tracker tracker)
	{
		map.GetComponent<TrackerComponent>().LoadTracker(tracker);
	}
}
