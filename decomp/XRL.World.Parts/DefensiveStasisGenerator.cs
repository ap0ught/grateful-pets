using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class DefensiveStasisGenerator : IPoweredPart
{
	public int Chance = 100;

	public string Duration = "1";

	public string Blueprint = "Stasisfield";

	public string AppearVerb = "appear";

	public string AppearExtra;

	public string AppearEndMark;

	public bool InterruptsTrigger = true;

	public bool PhaseSync = true;

	public DefensiveStasisGenerator()
	{
		WorksOnEquipper = true;
		IsRealityDistortionBased = true;
	}

	public override bool SameAs(IPart p)
	{
		DefensiveStasisGenerator defensiveStasisGenerator = p as DefensiveStasisGenerator;
		if (defensiveStasisGenerator.Chance != Chance)
		{
			return false;
		}
		if (defensiveStasisGenerator.Duration != Duration)
		{
			return false;
		}
		if (defensiveStasisGenerator.Blueprint != Blueprint)
		{
			return false;
		}
		if (defensiveStasisGenerator.AppearVerb != AppearVerb)
		{
			return false;
		}
		if (defensiveStasisGenerator.AppearExtra != AppearExtra)
		{
			return false;
		}
		if (defensiveStasisGenerator.AppearEndMark != AppearEndMark)
		{
			return false;
		}
		if (defensiveStasisGenerator.InterruptsTrigger != InterruptsTrigger)
		{
			return false;
		}
		if (defensiveStasisGenerator.PhaseSync != PhaseSync)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (!E.Damage.HasAttribute("Unstoppable") && IsObjectActivePartSubject(E.Object))
		{
			Cell cell = E.Object.CurrentCell;
			if (cell != null && !cell.HasObject(Blueprint))
			{
				int num = Duration.RollCached();
				if (num > 0 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					GameObject gameObject = ParentObject.Equipped ?? ParentObject.Implantee ?? ParentObject;
					GameObject gameObject2 = GameObject.Create(Blueprint);
					Forcefield part = gameObject2.GetPart<Forcefield>();
					if (part != null && gameObject != null)
					{
						part.Creator = gameObject;
					}
					if (num != 9999)
					{
						gameObject2.AddPart(new Temporary(num + 1));
					}
					if (PhaseSync && gameObject != null)
					{
						Phase.carryOver(gameObject, gameObject2);
					}
					cell.AddObject(gameObject2);
					IComponent<GameObject>.XDidY(gameObject2, AppearVerb, AppearExtra, AppearEndMark, null, null, null, null, UseFullNames: false, IndefiniteSubject: true);
					if (InterruptsTrigger)
					{
						return false;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("glass", 1);
			E.Add("ice", 1);
		}
		return base.HandleEvent(E);
	}
}
