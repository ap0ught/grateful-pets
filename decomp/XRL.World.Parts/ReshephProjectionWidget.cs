using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ReshephProjectionWidget : IPart
{
	private List<HologramProjector> Projectors;

	private bool Registered;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckTakeover();
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		GameManager.Instance.gameQueue.queueTask(CheckTakeover);
		return base.HandleEvent(E);
	}

	public void RegisterProjectors()
	{
		if (Registered || !ParentObject.CurrentZone.Built)
		{
			return;
		}
		Zone.ObjectEnumerator enumerator = ParentObject.CurrentZone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.TryGetPart<HologramProjector>(out var Part))
			{
				if (Projectors == null)
				{
					Projectors = new List<HologramProjector>();
				}
				Projectors.Add(Part);
				current.RegisterEvent(this, PowerSwitchFlippedEvent.ID, EventOrder.LATE);
			}
		}
		Registered = true;
	}

	public void CheckTakeover()
	{
		RegisterProjectors();
		if (!Registered || Projectors.IsNullOrEmpty())
		{
			return;
		}
		int num = 0;
		bool flag = false;
		foreach (HologramProjector projector in Projectors)
		{
			if (projector.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: true, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				num++;
			}
			if (projector.Blueprint == "Resheph Hologram")
			{
				flag = true;
			}
		}
		bool flag2 = num == Projectors.Count;
		if (flag == flag2)
		{
			return;
		}
		foreach (HologramProjector projector2 in Projectors)
		{
			GameObject parentObject = projector2.ParentObject;
			string blueprint = projector2.Blueprint;
			if (flag2)
			{
				blueprint = ((projector2.Blueprint == "Archon Hologram") ? "Resheph Hologram" : null);
			}
			else
			{
				blueprint = parentObject.GetBlueprint().GetPartParameter("HologramProjector", "Blueprint", blueprint);
			}
			projector2.SetBlueprint(blueprint, Silent: true);
			parentObject.Sparksplatter();
			parentObject.PlayWorldSound("sfx_spark");
		}
	}
}
