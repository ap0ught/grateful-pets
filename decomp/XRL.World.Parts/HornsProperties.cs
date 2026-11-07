using System;
using System.Text;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class HornsProperties : IPart
{
	public int HornLevel;

	public override bool SameAs(IPart p)
	{
		if ((p as HornsProperties).HornLevel != HornLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetToHitModifierEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Weapon == ParentObject && E.Checking == "Actor")
		{
			E.Modifier += GetToHitBonus();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		GetBleedingPerformance(out var Damage, out var SaveTarget);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("+" + GetToHitBonus() + " to hit").AppendLine().Append("On penetration, this weapon causes bleeding: ")
			.Append(Damage)
			.Append(" damage per round; save difficulty ")
			.Append(SaveTarget)
			.Append(".");
		E.Postfix.AppendRules(stringBuilder.ToString());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Intrinsic && !E.Primary && E.Weapon == ParentObject)
		{
			E.SetFinalizedChance(E.Properties.HasDelimitedSubstring(',', "Charging") ? 100 : 20);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "HornLevel", HornLevel);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponDealDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage" && E.GetIntParameter("Penetrations") > 0)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null)
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
				GetBleedingPerformance(out var Damage, out var SaveTarget);
				gameObjectParameter.ApplyEffect(new Bleeding(Damage, SaveTarget, gameObjectParameter2));
			}
		}
		return base.FireEvent(E);
	}

	public void GetBleedingPerformance(out string Damage, out int SaveTarget)
	{
		int hornLevel = GetHornLevel();
		Damage = "1";
		if (hornLevel > 4)
		{
			Damage = "1d2";
			int num = (hornLevel - 4) / 4;
			if (num > 0)
			{
				Damage += num.Signed();
			}
		}
		SaveTarget = 20 + 2 * hornLevel;
	}

	public int GetToHitBonus()
	{
		return GetHornLevel() / 2 + 1;
	}

	public static int GetToHitBonus(int Level)
	{
		return Level / 2 + 1;
	}

	public int GetHornLevel()
	{
		int result = 1;
		if (HornLevel != 0)
		{
			result = HornLevel;
		}
		else
		{
			Mutations mutations = ParentObject?.Equipped?.GetPart<Mutations>();
			if (mutations != null && mutations.GetMutation("Horns") is Horns horns)
			{
				result = horns.Level;
			}
		}
		return result;
	}
}
