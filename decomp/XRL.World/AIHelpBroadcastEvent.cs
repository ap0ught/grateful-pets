using System.Collections.Generic;
using XRL.World.AI;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 64, Cache = Cache.Pool)]
public class AIHelpBroadcastEvent : PooledEvent<AIHelpBroadcastEvent>
{
	public new static readonly int CascadeLevel = 64;

	public GameObject Actor;

	public GameObject Target;

	public GameObject Item;

	public List<string> Factions = new List<string>();

	public HelpCause Cause;

	public float Magnitude = 1f;

	private List<GameObject> Flood = new List<GameObject>();

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Target = null;
		Item = null;
		Factions.Clear();
		Cause = HelpCause.General;
		Magnitude = 1f;
		Flood.Clear();
	}

	public static void Send(GameObject Actor, GameObject Target, GameObject Item = null, string Faction = null, int Radius = 20, float Magnitude = 1f, HelpCause Cause = HelpCause.General)
	{
		Cell currentCell = Actor.CurrentCell;
		if (currentCell == null)
		{
			return;
		}
		AIHelpBroadcastEvent E = PooledEvent<AIHelpBroadcastEvent>.FromPool();
		E.Actor = Actor;
		E.Target = Target;
		E.Item = Item;
		E.Cause = Cause;
		E.Magnitude = Magnitude;
		AllegianceSet allegianceSet = Actor.Brain?.Allegiance;
		if (allegianceSet != null)
		{
			foreach (KeyValuePair<string, int> item in allegianceSet)
			{
				if (Brain.GetAllegianceLevel(item.Value) == Brain.AllegianceLevel.Member)
				{
					E.Factions.Add(item.Key);
				}
			}
		}
		string text = Actor.Physics?.Owner;
		if (!text.IsNullOrEmpty() && !E.Factions.Contains(text))
		{
			E.Factions.Add(text);
		}
		if (!Faction.IsNullOrEmpty() && !E.Factions.Contains(Faction))
		{
			E.Factions.Add(Faction);
		}
		E.Flood.Clear();
		currentCell.ParentZone.FastFloodVisibility(currentCell.X, currentCell.Y, 20, E.Flood, typeof(Brain), Actor);
		GameObject gameObject = Actor.Brain?.GetFinalLeader();
		if (gameObject?.Brain != null && gameObject.HandleEvent(E))
		{
			gameObject.Brain.HandleEvent(E);
		}
		foreach (GameObject item2 in E.Flood)
		{
			if (item2 != Actor && item2 != gameObject && item2.HandleEvent(E))
			{
				item2.Brain.HandleEvent(E);
			}
		}
		PooledEvent<AIHelpBroadcastEvent>.ResetTo(ref E);
	}
}
