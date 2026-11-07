using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasFungalSpores : IObjectGasBehavior
{
	public string Infection = "LuminousInfection";

	public string GasType = "FungalSpores";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasFungalSpores gasFungalSpores = p as GasFungalSpores;
		if (gasFungalSpores.Infection != Infection)
		{
			return false;
		}
		if (gasFungalSpores.GasType != GasType)
		{
			return false;
		}
		if (gasFungalSpores.GasLevel != GasLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAdjacentNavigationWeightEvent.ID)
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor != ParentObject.GetPart<Gas>()?.Creator && E.Actor.FireEvent("CanApplySpores") && !E.Actor.HasEffect<FungalSporeInfection>() && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 2 + 20, 80);
				}
			}
			else
			{
				E.MinWeight(10);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor != ParentObject.GetPart<Gas>()?.Creator && E.Actor.FireEvent("CanApplySpores") && !E.Actor.HasEffect<FungalSporeInfection>() && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 10 + 4, 16);
				}
			}
			else
			{
				E.MinWeight(2);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool ApplyGas(GameObject Object)
	{
		if (Object == ParentObject)
		{
			return false;
		}
		GameObject gameObject = null;
		Gas part = ParentObject.GetPart<Gas>();
		if (part != null)
		{
			gameObject = part.Creator;
		}
		if (Object == gameObject)
		{
			return false;
		}
		if (!CheckGasCanAffectEvent.Check(Object, ParentObject, part))
		{
			return false;
		}
		if (!Object.FireEvent("CanApplySpores"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplySpores"))
		{
			return false;
		}
		if (Object.HasTagOrProperty("ImmuneToFungus"))
		{
			return false;
		}
		if (!Object.PhaseMatches(ParentObject))
		{
			return false;
		}
		Object.RemoveEffect<SporeCloudPoison>();
		if (!Object.HasStat("Toughness"))
		{
			return false;
		}
		if (Object.HasEffect<FungalSporeInfection>())
		{
			return false;
		}
		bool result = false;
		SporeCloudPoison sporeCloudPoison = Object.GetEffect<SporeCloudPoison>();
		if (Infection == "PaxInfection")
		{
			GasLevel = 8;
		}
		else
		{
			int num = Stat.Random(2, 5);
			bool flag = sporeCloudPoison != null;
			if (sporeCloudPoison == null)
			{
				sporeCloudPoison = new SporeCloudPoison(num, part?.Creator);
			}
			else
			{
				if (sporeCloudPoison.Duration < num)
				{
					sporeCloudPoison.Duration = num;
				}
				if (!GameObject.Validate(ref sporeCloudPoison.Owner) && part != null && part.Creator != null)
				{
					sporeCloudPoison.Owner = part.Creator;
				}
			}
			sporeCloudPoison.Damage = Math.Min(1, GasLevel);
			if (flag || Object.ApplyEffect(sporeCloudPoison))
			{
				result = true;
			}
		}
		int difficulty = 10 + GasLevel / 3;
		if (!Object.MakeSave("Toughness", difficulty, null, null, "Fungal Disease Contact Gas", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your skin itches.");
			}
			Event obj = new Event("BeforeApplyFungalInfection");
			bool flag2 = false;
			if (!Object.FireEvent(obj) || obj.HasParameter("Cancelled"))
			{
				flag2 = true;
			}
			FungalSporeInfection fungalSporeInfection;
			if (Infection == "PaxInfection")
			{
				fungalSporeInfection = new FungalSporeInfection(3, Infection);
			}
			else
			{
				fungalSporeInfection = new FungalSporeInfection(flag2 ? Stat.Random(8, 10) : (Stat.Random(20, 30) * 120), Infection);
				fungalSporeInfection.Fake = flag2;
			}
			if (Object.ApplyEffect(fungalSporeInfection))
			{
				result = true;
			}
		}
		return result;
	}
}
