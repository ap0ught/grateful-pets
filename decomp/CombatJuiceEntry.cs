using System.Collections.Generic;

public class CombatJuiceEntry
{
	public long turn;

	public float duration;

	public float t;

	public bool async;

	public bool finished;

	public List<CombatJuiceEntry> delayedEntries = new List<CombatJuiceEntry>();

	public static GameManager gameManager => GameManager.Instance;

	public virtual bool canStart()
	{
		return true;
	}

	public virtual bool canFinishUpToTurn()
	{
		return true;
	}

	public virtual void start()
	{
	}

	public virtual void update()
	{
	}

	public virtual void finish()
	{
		if (delayedEntries.Count <= 0)
		{
			return;
		}
		foreach (CombatJuiceEntry delayedEntry in delayedEntries)
		{
			CombatJuiceManager.enqueueEntry(delayedEntry, delayedEntry.async);
		}
		delayedEntries.Clear();
	}
}
