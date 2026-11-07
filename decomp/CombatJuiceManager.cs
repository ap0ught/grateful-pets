using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatJuiceManager : MonoBehaviour
{
	public List<CombatJuiceEntry> active = new List<CombatJuiceEntry>();

	public List<CombatJuiceEntry> waiting = new List<CombatJuiceEntry>();

	public Queue<CombatJuiceEntry> queue = new Queue<CombatJuiceEntry>();

	public Dictionary<Type, Queue<CombatJuiceEntry>> pool = new Dictionary<Type, Queue<CombatJuiceEntry>>();

	public static bool NoPause = false;

	public GameManager gameManager;

	public static CombatJuiceManager instance;

	private static List<CombatJuiceEntry> delayedSequence = new List<CombatJuiceEntry>();

	private static bool delaying = false;

	private static List<CombatJuiceEntry> finishedEntries = new List<CombatJuiceEntry>();

	public ex3DSprite2[,] tiles => gameManager.ConsoleCharacter;

	public static bool AnyActive()
	{
		return instance.active.Count > 0;
	}

	public void Awake()
	{
		instance = this;
		gameObject.transform.position = new Vector3(0f, 0f, 0f);
	}

	public static void clearUpToTurn(long n)
	{
		lock (instance.queue)
		{
			foreach (CombatJuiceEntry item in instance.active)
			{
				try
				{
					if (item.canFinishUpToTurn())
					{
						item.finish();
						item.finished = true;
					}
					else
					{
						item.async = true;
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("clearUpToTurn::entry::finish", x);
					if (item != null)
					{
						item.finished = true;
					}
				}
			}
			instance.active.RemoveAll((CombatJuiceEntry e) => e.finished);
			while (instance.queue.Count > 0 && instance.queue.Peek().turn <= n)
			{
				instance.queue.Dequeue();
			}
			instance.waiting.RemoveAll((CombatJuiceEntry e) => e.turn <= n);
		}
	}

	public static void finishAll()
	{
		if (instance == null)
		{
			return;
		}
		foreach (CombatJuiceEntry item in instance.active)
		{
			item.finish();
			item.finished = true;
		}
		instance.waiting.Clear();
		instance.queue.Clear();
		instance.active.Clear();
		if (CombatJuice.roots == null || !CombatJuice.roots.ContainsKey("_JuiceRoot"))
		{
			return;
		}
		foreach (Transform item2 in CombatJuice.roots["_JuiceRoot"].transform)
		{
			foreach (Transform item3 in item2)
			{
				item3.gameObject.SendMessage("Finish", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static void pause()
	{
		clearUpToTurn(CombatJuice.juiceTurn - 1);
	}

	public static void startDelay()
	{
		_ = delaying;
		delaying = true;
		delayedSequence.Clear();
	}

	public static void endDelay()
	{
		delaying = false;
	}

	public static void enqueueEntry(CombatJuiceEntry newEntry, bool async = false)
	{
		if (delaying)
		{
			newEntry.async = async;
			newEntry.turn = CombatJuice.juiceTurn;
			delayedSequence.Add(newEntry);
			return;
		}
		if (delayedSequence.Count > 0)
		{
			newEntry.delayedEntries.Clear();
			newEntry.delayedEntries.AddRange(delayedSequence);
			delayedSequence.Clear();
		}
		lock (instance.queue)
		{
			newEntry.turn = CombatJuice.juiceTurn;
			newEntry.async = async;
			if (async)
			{
				instance.waiting.Add(newEntry);
			}
			else
			{
				instance.queue.Enqueue(newEntry);
			}
		}
	}

	private static void begin(CombatJuiceEntry effect)
	{
		if (!effect.canStart())
		{
			instance.waiting.Add(effect);
			return;
		}
		effect.start();
		effect.update();
		if (effect.t < effect.duration)
		{
			instance.active.Add(effect);
			return;
		}
		effect.finish();
		effect.finished = true;
	}

	public static void update()
	{
		finishedEntries.Clear();
		if (instance.active.Count > 0)
		{
			foreach (CombatJuiceEntry item in instance.active)
			{
				item.t += Time.deltaTime;
				try
				{
					item.update();
				}
				catch (Exception x)
				{
					MetricsManager.LogException("CombatJuiceActiveUpdate", x);
					item.t = item.duration + float.Epsilon;
				}
				if (item.t > item.duration)
				{
					try
					{
						item.finish();
					}
					catch (Exception x2)
					{
						MetricsManager.LogException("CombatJuiceActiveFinish", x2);
					}
					item.finished = true;
					finishedEntries.Add(item);
				}
			}
		}
		foreach (CombatJuiceEntry finishedEntry in finishedEntries)
		{
			instance.active.Remove(finishedEntry);
		}
		if (instance.queue.Count <= 0 && instance.waiting.Count <= 0)
		{
			return;
		}
		lock (instance.queue)
		{
			for (int i = 0; i < instance.waiting.Count; i++)
			{
				CombatJuiceEntry combatJuiceEntry = instance.waiting[i];
				if (combatJuiceEntry.canStart())
				{
					try
					{
						begin(combatJuiceEntry);
					}
					catch (Exception x3)
					{
						MetricsManager.LogException("CombatJuiceWaitingBegin", x3);
						instance.active.Remove(combatJuiceEntry);
					}
					instance.waiting.RemoveAt(i);
					i--;
				}
			}
			while (!AnyNonAsync() && instance.queue.Count > 0)
			{
				CombatJuiceEntry combatJuiceEntry2 = instance.queue.Dequeue();
				try
				{
					begin(combatJuiceEntry2);
				}
				catch (Exception x4)
				{
					MetricsManager.LogException("CombatJuiceQueueBegin", x4);
					instance.active.Remove(combatJuiceEntry2);
					instance.waiting.Remove(combatJuiceEntry2);
				}
			}
		}
	}

	public static bool AnyNonAsync()
	{
		for (int num = instance.active.Count - 1; num >= 0; num--)
		{
			if (!instance.active[num].async)
			{
				return true;
			}
		}
		return false;
	}
}
