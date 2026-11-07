using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingHandOff : IPart
{
	public string SaveStat = "Strength";

	public string SaveDifficultyStat = "Strength";

	public int SaveTarget = 30;

	public string BleedingDamageBonus;

	public int BleedingSavePenalty;

	public override bool SameAs(IPart Part)
	{
		return false;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (ParentObject.TryGetPart<Engulfing>(out var Part))
		{
			if (Part.Engulfed == null)
			{
				return true;
			}
			if (!Part.CheckEngulfed())
			{
				return true;
			}
			if (!Part.Engulfed.CanBeInvoluntarilyMoved())
			{
				return true;
			}
			List<Engulfing> adjacentReceivers = GetAdjacentReceivers();
			if (adjacentReceivers.Count > 0)
			{
				AttemptHandOff(Part, adjacentReceivers.GetRandomElement(), Part.Engulfed);
			}
		}
		return base.HandleEvent(E);
	}

	public List<Engulfing> GetAdjacentReceivers()
	{
		List<Engulfing> list = new List<Engulfing>();
		ParentObject.CurrentCell?.ForeachAdjacentCell((Cell Cell) =>
		{
			foreach (GameObject @object in Cell.Objects)
			{
				if (@object.TryGetPart<Engulfing>(out var Part))
				{
					if (Part.Engulfed == null)
					{
						list.Add(Part);
					}
					break;
				}
			}
		});
		return list;
	}

	public bool HandOffSave(GameObject Engulfed)
	{
		return Engulfed.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat, "Engulfment Move");
	}

	public void AttemptHandOff(Engulfing Giver, Engulfing Receiver, GameObject Engulfed)
	{
		ParentObject.UseEnergy(1000, "HandOff");
		if (!HandOffSave(Engulfed) && Engulfed.TryGetEffect<Engulfed>(out var Effect))
		{
			Giver.Engulfed = null;
			Effect.EngulfedBy = Receiver.ParentObject;
			Engulfed.ApplyEffect(new AnemoneEffect(BleedingDamageBonus, BleedingSavePenalty));
			Engulfed.CurrentCell.RemoveObject(Engulfed);
			Receiver.ParentObject.CurrentCell.AddObject(Engulfed);
			Receiver.Engulfed = Engulfed;
			Receiver.ParentObject.UseEnergy(1000, "HandOff");
			if (ParentObject.IsPlayer() || Engulfed.IsPlayer())
			{
				DidXToY("hand", Engulfed, "off", null, null, null, null, Engulfed);
			}
		}
	}
}
