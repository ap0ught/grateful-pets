using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Cleave : BaseSkill
{
	[NonSerialized]
	private static int Penalty;

	[NonSerialized]
	private static string PenaltyEffect;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterAttack");
		base.Register(Object, Registrar);
	}

	private static void ProcessPenalty(Effect FX)
	{
		if (FX is IShatterEffect shatterEffect)
		{
			Penalty += shatterEffect.GetPenalty();
		}
	}

	private static void ProcessPenalties(GameObject obj)
	{
		foreach (Effect effect in obj.Effects)
		{
			if (effect is IShatterEffect && effect.ClassName == PenaltyEffect)
			{
				ProcessPenalty(effect);
			}
		}
	}

	public static int GetCurrentPenalty(GameObject who, string EffectName, string EquipmentEffectName)
	{
		Penalty = 0;
		if (EffectName != null)
		{
			PenaltyEffect = EffectName;
			ProcessPenalties(who);
		}
		if (EquipmentEffectName != null)
		{
			PenaltyEffect = EquipmentEffectName;
			who.ForeachEquippedObject(ProcessPenalties);
		}
		return Penalty;
	}

	public static void PerformCleave(GameObject Attacker, GameObject Defender, GameObject Weapon, string Skill = null, string Properties = null, int Chance = 75, int AdjustAVPenalty = 0, int? MaxAVPenalty = null)
	{
		if (Attacker == null || Defender == null || Weapon == null || Defender.HasPart<Gas>() || Defender.HasPart<NoDamage>())
		{
			return;
		}
		MeleeWeapon part = Weapon.GetPart<MeleeWeapon>();
		if (Skill != null && (part == null || part.Skill != Skill))
		{
			return;
		}
		string stat = part.Stat;
		string name;
		string text;
		string text2;
		string equipmentEffectName;
		string iD;
		if (Statistic.IsMental(stat))
		{
			name = "MA";
			text = "mental armor";
			text2 = "ShatterMentalArmor";
			equipmentEffectName = null;
			iD = "CanApplyShatterMentalArmor";
		}
		else
		{
			name = "AV";
			text = "armor";
			text2 = "ShatterArmor";
			equipmentEffectName = "ShatteredArmor";
			iD = "CanApplyShatterArmor";
		}
		if (Defender == null || !Defender.HasStat(name) || !Defender.FireEvent(iD) || !CanApplyEffectEvent.Check(Defender, text2))
		{
			return;
		}
		int num = Attacker.StatMod(stat);
		int num2;
		if (!MaxAVPenalty.HasValue)
		{
			num2 = num / 2 + AdjustAVPenalty;
			if (num % 2 == 1)
			{
				num2++;
			}
		}
		else
		{
			num2 = MaxAVPenalty.Value + AdjustAVPenalty;
		}
		if (num2 < 1)
		{
			num2 = 1;
		}
		int num3 = 1;
		bool flag = Properties != null && Properties.HasDelimitedSubstring(',', "Charging") && Attacker.HasSkill("Cudgel_ChargingStrike");
		if (flag)
		{
			num3++;
		}
		int num4 = GetCleaveAmountEvent.GetFor(Weapon, Attacker, Defender, num3);
		if (num4 <= 0)
		{
			return;
		}
		if (num2 < num4)
		{
			num2 = num4;
		}
		num2 = GetCleaveMaxPenaltyEvent.GetFor(Weapon, Attacker, Defender, num2, flag);
		if (num2 <= 0)
		{
			return;
		}
		Statistic stat2 = Defender.GetStat(name);
		int num5 = GetCurrentPenalty(Defender, text2, equipmentEffectName);
		if (stat2.Value <= stat2.Min || num5 >= num2)
		{
			return;
		}
		GetSpecialEffectChanceEvent.GetFor(Attacker, Weapon, "Skill Cleave", Chance, Defender);
		if (!Chance.in100())
		{
			return;
		}
		bool flag2 = false;
		if (Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text2)) is IShatterEffect shatterEffect)
		{
			shatterEffect.Duration = 300;
			shatterEffect.SetOwner(Attacker);
			for (int i = shatterEffect.GetPenalty(); i < num4; i++)
			{
				if (num5 >= num2 - 1)
				{
					break;
				}
				shatterEffect.IncrementPenalty();
				num5++;
			}
			flag2 = Defender.ApplyEffect(shatterEffect);
		}
		if (!flag2)
		{
			return;
		}
		if (flag)
		{
			if (Attacker.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("The momentum from your charge causes your " + Weapon.ShortDisplayNameSingle + " to cleave deeper through " + Defender.poss(text) + ".", 'g');
			}
			Defender.DustPuff("&c");
		}
		if (Attacker.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You cleave through " + Defender.poss(text) + ".", 'G');
		}
		else if (Defender.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(Attacker.Does("cleave") + " through your " + text + ".", 'R');
		}
		else if (IComponent<GameObject>.Visible(Defender))
		{
			if (Defender.IsPlayerLed())
			{
				IComponent<GameObject>.AddPlayerMessage(Attacker.Does("cleave") + " through " + Defender.poss(text) + ".", 'r');
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(Attacker.Does("cleave") + " through " + Defender.poss(text) + ".", 'g');
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterAttack" && !E.HasFlag("Critical"))
		{
			PerformCleave(E.GetGameObjectParameter("Attacker"), E.GetGameObjectParameter("Defender"), E.GetGameObjectParameter("Weapon"), "Axe", E.GetStringParameter("Properties"));
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return base.RemoveSkill(GO);
	}
}
