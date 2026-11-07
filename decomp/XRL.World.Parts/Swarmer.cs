using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class Swarmer : IPart
{
	public int ExtraBonus;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != GetAttackerMeleePenetrationEvent.ID)
		{
			return ID == PooledEvent<GetToHitModifierEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ExtraBonus > 0)
		{
			E.Postfix.AppendRules("Swarm Alpha: As long as this creature is adjacent to " + ParentObject.its + " target, " + ParentObject.it + ParentObject.GetVerb("grant", PrependSpace: true, PronounAntecedent: true) + " " + ExtraBonus + " to the swarm bonuses of each other swarmer who is adjacent to " + ParentObject.its + " target.");
		}
		int swarmBonus = GetSwarmBonus();
		E.Postfix.AppendRules("Swarmer: This creature receives +1 to hit in melee and +1 to penetration rolls for each other hostile swarmer beyond the first who is in another square adjacent to " + ParentObject.its + " target. (currently " + swarmBonus.Signed() + ")");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAttackerMeleePenetrationEvent E)
	{
		E.PenetrationBonus += GetSwarmBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Actor == ParentObject && E.Checking == "Actor" && E.Melee)
		{
			E.Modifier += GetSwarmBonus();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void FindSwarmers(Cell C, GameObject Target, ref int swarmers, ref int extra)
	{
		bool flag = false;
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (gameObject == ParentObject)
			{
				continue;
			}
			Swarmer part = gameObject.GetPart<Swarmer>();
			if (part != null && gameObject.IsHostileTowards(Target))
			{
				flag = true;
				if (part.ExtraBonus > extra)
				{
					extra = part.ExtraBonus;
				}
			}
		}
		if (flag)
		{
			swarmers++;
		}
	}

	public int GetSwarmBonus()
	{
		Cell cell = ParentObject.CurrentCell;
		GameObject gameObject = ParentObject.Target ?? IComponent<GameObject>.ThePlayer;
		if (gameObject != null && ParentObject.DistanceTo(gameObject) <= 1)
		{
			Cell cell2 = gameObject.CurrentCell;
			if (cell2 != null)
			{
				int swarmers = -1;
				int extra = 0;
				if (cell2 != cell)
				{
					FindSwarmers(cell2, gameObject, ref swarmers, ref extra);
				}
				List<Cell> adjacentCells = cell2.GetAdjacentCells();
				int i = 0;
				for (int count = adjacentCells.Count; i < count; i++)
				{
					if (adjacentCells[i] != cell)
					{
						FindSwarmers(adjacentCells[i], gameObject, ref swarmers, ref extra);
					}
				}
				swarmers = ((swarmers > 0 || extra <= 0) ? (swarmers + extra) : extra);
				if (swarmers > 0)
				{
					return swarmers;
				}
			}
		}
		return 0;
	}
}
