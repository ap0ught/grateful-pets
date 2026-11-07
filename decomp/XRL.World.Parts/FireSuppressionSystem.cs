using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FireSuppressionSystem : IPoweredPart
{
	public int Chance = 100;

	public string Liquid = "gel";

	public string Amount = "2-4";

	public string Sound = "Sounds/Abilities/sfx_ability_thickLiquidSpray";

	public FireSuppressionSystem()
	{
		WorksOnSelf = true;
		ChargeUse = 0;
	}

	public override bool SameAs(IPart p)
	{
		FireSuppressionSystem fireSuppressionSystem = p as FireSuppressionSystem;
		if (fireSuppressionSystem.Amount != Amount)
		{
			return false;
		}
		if (fireSuppressionSystem.Sound != Sound)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount1)
	{
		CheckFireSuppression();
	}

	public int CheckFireSuppression()
	{
		int result = 0;
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				CheckFireSuppression(activePartSubject);
			}
		}
		else
		{
			CheckFireSuppression(GetActivePartFirstSubject());
		}
		return result;
	}

	public bool CheckFireSuppression(GameObject obj)
	{
		if (GameObject.Validate(ref obj) && obj.IsAflame() && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = Amount.RollCached();
			if (num > 0)
			{
				string name = LiquidVolume.GetLiquid(Liquid).GetName(null);
				if (obj.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(num + " " + ((num == 1) ? "dram" : "drams") + " of " + name + " discharges all over you.");
				}
				else if (IComponent<GameObject>.Visible(obj))
				{
					IComponent<GameObject>.AddPlayerMessage(num + " " + ((num == 1) ? "dram" : "drams") + " of " + name + " discharges all over " + obj.the + obj.ShortDisplayName + ".");
				}
				obj.ApplyEffect(new LiquidCovered(Liquid, num));
				obj.LiquidSplash(LiquidVolume.GetLiquid(Liquid));
				PlayWorldSound(Sound);
				return true;
			}
		}
		return false;
	}
}
