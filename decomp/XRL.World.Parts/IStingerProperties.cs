using System;
using System.Text;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public abstract class IStingerProperties : IPart
{
	public const string BASE_SAVE_VS = "Stinger Injected Poison";

	public const string BASE_SAVE_ATTR = "Toughness";

	[NonSerialized]
	private Stinger Stinger;

	public virtual string SaveVs => "Stinger Injected Poison";

	public virtual string SaveAttribute => "Toughness";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetMeleeAttackChanceEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Intrinsic && !E.Primary && E.Weapon == ParentObject)
		{
			bool flag = E.Properties.HasDelimitedSubstring(',', "Charging") || E.Properties.HasDelimitedSubstring(',', "Lunging") || E.Properties.HasDelimitedSubstring(',', "Flurrying");
			E.SetFinalizedChance(flag ? 100 : 20);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponDealDamage");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			string stringParameter = E.GetStringParameter("Properties", "");
			if (Stinger != null || gameObjectParameter.TryGetPart<Stinger>(out Stinger))
			{
				int chance = 20;
				if (!ParentObject.IsEquippedOrDefaultOfPrimary(gameObjectParameter) || stringParameter.HasDelimitedSubstring(',', "Stinging") || stringParameter.HasDelimitedSubstring(',', "Charging") || stringParameter.HasDelimitedSubstring(',', "Lunging"))
				{
					chance = 100;
				}
				if (chance.in100())
				{
					int level = Stinger.Level;
					ApplyPoison(Stinger, level, gameObjectParameter, gameObjectParameter2);
				}
			}
		}
		return base.FireEvent(E);
	}

	public abstract Effect CreateEffect(GameObject Attacker, GameObject Defender, int Level);

	public virtual void FailureMessage(GameObject Attacker, GameObject Defender, Effect Effect)
	{
		if (Attacker.IsPlayer() || Defender.IsPlayer())
		{
			IComponent<GameObject>.XDidY(Defender, "resist", "the effects of " + Attacker.poss("venom"), "!", null, null, Defender);
		}
	}

	public virtual int GetCooldown(int Level)
	{
		return 25;
	}

	public virtual string GetDamage(int Level)
	{
		if (Level < 10)
		{
			if (Level >= 4)
			{
				if (Level < 7)
				{
					return "1d8";
				}
				return "1d10";
			}
			return "1d6";
		}
		if (Level < 16)
		{
			if (Level < 13)
			{
				return "1d12";
			}
			return "2d6";
		}
		if (Level < 19)
		{
			return "2d6+1";
		}
		return "2d8";
	}

	public virtual int GetPenetration(int Level)
	{
		if (Level >= 2)
		{
			return Math.Min(9, (Level - 2) / 3 + 4);
		}
		return 3;
	}

	public virtual string GetDuration(int Level)
	{
		return "";
	}

	public virtual string GetDescription()
	{
		return "You bear a tail with a stinger that delivers " + GetAdjective() + " venom to your enemies.";
	}

	public virtual string GetAdjective()
	{
		return "poisonous";
	}

	public virtual void SuccessMessage(GameObject Attacker, GameObject Defender, Effect Effect)
	{
	}

	public virtual void AppendLevelText(StringBuilder SB, int Level)
	{
	}

	public virtual void VisualEffect(GameObject Attacker, GameObject Defender)
	{
		Defender.Splatter("&g.");
	}

	public virtual void ApplyPoison(Stinger Mutation, int Level, GameObject Attacker, GameObject Defender)
	{
		if (Defender.HasStat(SaveAttribute) && Defender.FireEvent("CanApplyPoison") && CanApplyEffectEvent.Check(Defender, "Poison"))
		{
			Effect effect = CreateEffect(Attacker, Defender, Level);
			if (Defender.FireEvent("ApplyPoison"))
			{
				string saveAttribute = SaveAttribute;
				int save = Mutation.GetSave(Level);
				string saveVs = SaveVs;
				if (!Defender.MakeSave(saveAttribute, save, Attacker, null, saveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject) && ApplyEffectEvent.Check(Defender, "Poison", effect, ParentObject) && Defender.ApplyEffect(effect, ParentObject))
				{
					SuccessMessage(Attacker, Defender, effect);
					goto IL_00b0;
				}
			}
			FailureMessage(Attacker, Defender, effect);
		}
		goto IL_00b0;
		IL_00b0:
		VisualEffect(Attacker, Defender);
	}
}
