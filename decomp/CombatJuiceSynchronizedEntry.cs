using System.Collections.Generic;
using UnityEngine;

public class CombatJuiceSynchronizedEntry : CombatJuiceEntry
{
	public List<CombatJuiceEntry> Entries;

	public CombatJuiceSynchronizedEntry()
	{
		Entries = new List<CombatJuiceEntry>();
	}

	public CombatJuiceSynchronizedEntry(IEnumerable<CombatJuiceEntry> Entries)
	{
		this.Entries = new List<CombatJuiceEntry>(Entries);
	}

	public override bool canStart()
	{
		foreach (CombatJuiceEntry entry in Entries)
		{
			if (!entry.canStart())
			{
				return false;
			}
		}
		return true;
	}

	public override void start()
	{
		foreach (CombatJuiceEntry entry in Entries)
		{
			entry.turn = turn;
			entry.start();
			duration = Mathf.Max(duration, entry.duration);
		}
	}

	public override void update()
	{
		foreach (CombatJuiceEntry entry in Entries)
		{
			entry.t = t;
			entry.update();
		}
	}

	public override void finish()
	{
		foreach (CombatJuiceEntry entry in Entries)
		{
			entry.finish();
			entry.finished = true;
		}
		base.finish();
	}
}
