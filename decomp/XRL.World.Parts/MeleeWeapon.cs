using System;
using System.CodeDom.Compiler;
using System.Text;
using Occult.Engine.CodeGeneration;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
[GeneratePoolingPartial]
[GenerateSerializationPartial]
public class MeleeWeapon : IPart
{
	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	private static readonly IPartPool MeleeWeaponPool = new IPartPool();

	public const int BONUS_CAP_UNLIMITED = 999;

	public int MaxStrengthBonus;

	public int PenBonus;

	public int HitBonus;

	public string BaseDamage = "5";

	public int Ego;

	public string Skill = "Cudgel";

	public string Stat = "Strength";

	public string Slot = "Hand";

	public string Attributes;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override IPartPool Pool => MeleeWeaponPool;

	[GeneratedCode("PoolPartialsGenerator", "1.0.0.0")]
	public override void Reset()
	{
		base.Reset();
		MaxStrengthBonus = 0;
		PenBonus = 0;
		HitBonus = 0;
		BaseDamage = "5";
		Ego = 0;
		Skill = "Cudgel";
		Stat = "Strength";
		Slot = "Hand";
		Attributes = null;
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(MaxStrengthBonus);
		Writer.WriteOptimized(PenBonus);
		Writer.WriteOptimized(HitBonus);
		Writer.WriteOptimized(BaseDamage);
		Writer.WriteOptimized(Ego);
		Writer.WriteOptimized(Skill);
		Writer.WriteOptimized(Stat);
		Writer.WriteOptimized(Slot);
		Writer.WriteOptimized(Attributes);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		MaxStrengthBonus = Reader.ReadOptimizedInt32();
		PenBonus = Reader.ReadOptimizedInt32();
		HitBonus = Reader.ReadOptimizedInt32();
		BaseDamage = Reader.ReadOptimizedString();
		Ego = Reader.ReadOptimizedInt32();
		Skill = Reader.ReadOptimizedString();
		Stat = Reader.ReadOptimizedString();
		Slot = Reader.ReadOptimizedString();
		Attributes = Reader.ReadOptimizedString();
	}

	public override bool SameAs(IPart p)
	{
		MeleeWeapon meleeWeapon = p as MeleeWeapon;
		if (meleeWeapon.MaxStrengthBonus != MaxStrengthBonus)
		{
			return false;
		}
		if (meleeWeapon.PenBonus != PenBonus)
		{
			return false;
		}
		if (meleeWeapon.HitBonus != HitBonus)
		{
			return false;
		}
		if (meleeWeapon.BaseDamage != BaseDamage)
		{
			return false;
		}
		if (meleeWeapon.Ego != Ego)
		{
			return false;
		}
		if (meleeWeapon.Skill != Skill)
		{
			return false;
		}
		if (meleeWeapon.Stat != Stat)
		{
			return false;
		}
		if (meleeWeapon.Slot != Slot)
		{
			return false;
		}
		if (meleeWeapon.Attributes != Attributes)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EquippedEvent.ID || Ego == 0) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetShortDescriptionEvent.ID && (ID != PooledEvent<GetToHitModifierEvent>.ID || HitBonus == 0) && ID != QueryEquippableListEvent.ID)
		{
			if (ID == UnequippedEvent.ID)
			{
				return Ego != 0;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetToHitModifierEvent E)
	{
		if (E.Weapon == ParentObject && E.Checking == "Actor")
		{
			E.Modifier += HitBonus;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		StatShifter.SetStatShift(E.Actor, "Ego", Ego);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		StatShifter.RemoveStatShifts(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (ParentObject.HasTagOrProperty("ShowMeleeWeaponStats") && E.Understood())
		{
			E.AddTag(GetSimplifiedStats(Options.ShowDetailedWeaponStats), -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		bool flag = ParentObject.HasTagOrProperty("ShowMeleeWeaponStats");
		if (flag)
		{
			if (Ego != 0)
			{
				E.Postfix.AppendRules(Ego.Signed() + " Ego");
			}
			if (HitBonus != 0)
			{
				E.Postfix.AppendRules(HitBonus.Signed() + " To-Hit");
			}
			E.Postfix.AppendRules(GetDetailedStats());
		}
		AppendOffhandChance(E.Postfix, flag);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.SlotType == Slot && E.Item == ParentObject && !E.List.Contains(E.Item) && (!E.RequireDesirable || (!IsImprovisedWeapon() && !E.Item.HasPropertyOrTag("UndesirableWeapon")) || E.Item.IsNatural() || (E.Item.HasPartDescendedFrom<ILightSource>() && !E.Item.HasPart<Armor>() && !E.Item.HasPart<Shield>() && !E.Item.HasPart<CyberneticsBaseItem>())))
		{
			if (!E.RequirePossible)
			{
				E.List.Add(E.Item);
			}
			else
			{
				string usesSlots = E.Item.UsesSlots;
				if (!usesSlots.IsNullOrEmpty() && (E.SlotType != "Thrown Weapon" || usesSlots.Contains("Thrown Weapon")) && (E.SlotType != "Hand" || usesSlots.Contains("Hand")))
				{
					if (E.Actor.IsGiganticCreature)
					{
						if (E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
						{
							E.List.Add(E.Item);
						}
					}
					else if (E.SlotType == "Hand" || E.SlotType == "Missile Weapon" || !E.Item.IsGiganticEquipment || !E.Item.IsNatural())
					{
						E.List.Add(E.Item);
					}
				}
				else if (!E.Actor.IsGiganticCreature || E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
				{
					int slotsRequiredFor = E.Item.GetSlotsRequiredFor(E.Actor, Slot, FloorAtOne: false);
					if (slotsRequiredFor > 0 && slotsRequiredFor <= E.Actor.GetBodyPartCount(E.SlotType))
					{
						E.List.Add(E.Item);
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
			if (Ego > 0 || MaxStrengthBonus >= 5)
			{
				E.Add("might", 1);
			}
			if (Stat == "Intelligence")
			{
				E.Add("scholarship", 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool AttackFromPart(BodyPart Part)
	{
		if (Part.PreferredPrimary)
		{
			return true;
		}
		if (string.IsNullOrEmpty(Slot))
		{
			return true;
		}
		if (Part.Type == null)
		{
			return true;
		}
		if (!(Slot == Part.Type))
		{
			return Slot == Part.VariantType;
		}
		return true;
	}

	public string GetDetailedStats()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (MaxStrengthBonus > 0 || PenBonus > 0)
		{
			stringBuilder.Compound(Stat, "\n").Append(" Bonus Cap: ");
			if (MaxStrengthBonus == 999)
			{
				stringBuilder.Append("no limit");
			}
			else
			{
				stringBuilder.Append(MaxStrengthBonus);
			}
		}
		string text = Skills.GetGenericSkill(Skill)?.GetWeaponCriticalDescription();
		if (!string.IsNullOrEmpty(text))
		{
			stringBuilder.Compound("Weapon Class: " + text, "\n");
		}
		return stringBuilder.ToString();
	}

	public void AppendOffhandChance(StringBuilder SB, bool Detailed = false)
	{
		GameObject gameObject = ParentObject.Equipped ?? The.Player;
		BodyPart bodyPart = gameObject?.Body?.FindDefaultOrEquippedItem(ParentObject);
		if (bodyPart == null)
		{
			return;
		}
		bool flag = bodyPart.Primary || gameObject.HasEquippedOnPrimary(ParentObject);
		int num = 0;
		if (AttackFromPart(bodyPart))
		{
			num = GetMeleeAttackChanceEvent.GetFor(gameObject, ParentObject, -1, 0, 1.0, null, bodyPart, bodyPart.Primary ? bodyPart : null, flag, Intrinsic: true);
		}
		if (flag)
		{
			if (num < 100)
			{
				SB.Append("\n{{rules|Attack Chance: ").Append(num).Append("%}}");
			}
		}
		else if (Detailed || num > 0)
		{
			SB.Append("\n{{rules|Offhand Attack Chance: ").Append(num).Append("%}}");
		}
	}

	public void GetNormalPenetration(GameObject Actor, out int BasePenetration, out int StatMod)
	{
		BasePenetration = PenBonus;
		StatMod = 0;
		if ((Skill == "LongBlades" || Skill == "ShortBlades") && Actor != null && Actor.HasPart(typeof(LongBladesCore)) && Actor.HasEffect(typeof(LongbladeStance_Aggressive)) && Actor.GetPart<LongBladesCore>().IsPrimaryBladeEquipped())
		{
			if (Actor.HasPart(typeof(LongBladesImprovedAggressiveStance)))
			{
				BasePenetration += 2;
			}
			else
			{
				BasePenetration++;
			}
		}
		if (Actor == null || HasTag("WeaponIgnoreStrength"))
		{
			return;
		}
		if (Stat.Contains(","))
		{
			StatMod = int.MinValue;
			foreach (string item in Stat.CachedCommaExpansion())
			{
				if (Actor.HasStat(item))
				{
					int num = Actor.StatMod(item);
					if (num > StatMod)
					{
						StatMod = num;
					}
				}
			}
			if (StatMod == int.MinValue)
			{
				StatMod = 0;
			}
		}
		else if (Actor.HasStat(Stat))
		{
			StatMod = Actor.StatMod(Stat);
		}
		if (StatMod > MaxStrengthBonus)
		{
			StatMod = MaxStrengthBonus;
		}
	}

	public int GetNormalPenetration(GameObject Actor)
	{
		GetNormalPenetration(Actor, out var BasePenetration, out var StatMod);
		return BasePenetration + StatMod;
	}

	public string GetSimplifiedStats(bool WithStrCapDetails)
	{
		GameObject actor = ParentObject.Equipped ?? IComponent<GameObject>.ThePlayer;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string value = GetDisplayNamePenetrationColorEvent.GetFor(ParentObject);
		if (!value.IsNullOrEmpty())
		{
			stringBuilder.Append("{{").Append(value).Append('|')
				.Append('\u001a')
				.Append("}}");
		}
		else
		{
			stringBuilder.Append('\u001a');
		}
		GetNormalPenetration(actor, out var BasePenetration, out var StatMod);
		int Bonus = 0;
		string Symbol = "÷";
		if (IsAdaptivePenetrationActiveEvent.Check(ParentObject, ref Bonus, ref Symbol))
		{
			stringBuilder.Append(Symbol);
			int num = BasePenetration + Bonus;
			if (num > 0)
			{
				stringBuilder.Append('+').Append(num);
			}
			else if (num < 0)
			{
				stringBuilder.Append(num);
			}
		}
		else
		{
			stringBuilder.Append(BasePenetration + StatMod + RuleSettings.VISUAL_PENETRATION_BONUS);
			if (WithStrCapDetails)
			{
				if (MaxStrengthBonus == 999)
				{
					stringBuilder.Append("{{K|/").Append('ì').Append("}}");
				}
				else
				{
					stringBuilder.Append("{{K|/").Append(BasePenetration + MaxStrengthBonus + RuleSettings.VISUAL_PENETRATION_BONUS).Append("}}");
				}
			}
		}
		stringBuilder.Append(" {{r|").Append('\u0003').Append("}}")
			.Append(BaseDamage);
		return stringBuilder.ToString();
	}

	public bool AdjustDamageDieSize(int Amount)
	{
		BaseDamage = DieRoll.AdjustDieSize(BaseDamage, Amount);
		DamageDieSizeAdjustedEvent.Send(ParentObject, this, Amount);
		return true;
	}

	public bool AdjustDamage(int Amount)
	{
		BaseDamage = DieRoll.AdjustResult(BaseDamage, Amount);
		DamageConstantAdjustedEvent.Send(ParentObject, this, Amount);
		return true;
	}

	public bool AdjustBonusCap(int Amount)
	{
		if (MaxStrengthBonus == 999)
		{
			return false;
		}
		MaxStrengthBonus += Amount;
		return true;
	}

	public bool IsEquippedOnPrimary()
	{
		if (ParentObject == null)
		{
			return false;
		}
		return ParentObject.Equipped?.HasEquippedOnPrimary(ParentObject) ?? false;
	}

	public bool IsImprovisedWeapon()
	{
		if (MaxStrengthBonus == 0 && PenBonus == 0 && HitBonus == 0 && BaseDamage == "1d2" && Ego == 0 && Skill == "Cudgel" && Stat == "Strength" && Slot == "Hand")
		{
			return Attributes.IsNullOrEmpty();
		}
		return false;
	}
}
