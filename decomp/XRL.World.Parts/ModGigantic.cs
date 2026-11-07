using System;
using System.Collections.Generic;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ModGigantic : IModification
{
	[NonSerialized]
	private bool HaveAddedWeight;

	[NonSerialized]
	private int AddedWeight;

	public ModGigantic()
	{
	}

	public ModGigantic(int Tier)
		: base(Tier)
	{
	}

	public ModGigantic(ModGigantic Source)
	{
	}

	public override int GetModificationSlotUsage()
	{
		return 0;
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "MassEnhancement";
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<MeleeWeapon>()?.AdjustDamage(3);
		EnergyCell part = Object.GetPart<EnergyCell>();
		if (part != null)
		{
			part.MaxCharge *= 2;
			part.Charge *= 2;
		}
		DeploymentGrenade part2 = Object.GetPart<DeploymentGrenade>();
		if (part2 != null)
		{
			part2.Radius *= 2;
		}
		EMPGrenade part3 = Object.GetPart<EMPGrenade>();
		if (part3 != null)
		{
			part3.Radius *= 2;
		}
		FlashbangGrenade part4 = Object.GetPart<FlashbangGrenade>();
		if (part4 != null)
		{
			part4.Radius *= 2;
		}
		GasGrenade part5 = Object.GetPart<GasGrenade>();
		if (part5 != null)
		{
			part5.Density *= 2;
		}
		GravityGrenade part6 = Object.GetPart<GravityGrenade>();
		if (part6 != null)
		{
			part6.Force *= 2;
			part6.Radius *= 2;
		}
		HEGrenade part7 = Object.GetPart<HEGrenade>();
		if (part7 != null)
		{
			part7.Force *= 2;
		}
		PhaseGrenade part8 = Object.GetPart<PhaseGrenade>();
		if (part8 != null)
		{
			part8.Radius *= 2;
		}
		SunderGrenade part9 = Object.GetPart<SunderGrenade>();
		if (part9 != null)
		{
			part9.Radius *= 2;
		}
		ThermalGrenade part10 = Object.GetPart<ThermalGrenade>();
		if (part10 != null)
		{
			part10.Radius *= 2;
		}
		TimeDilationGrenade part11 = Object.GetPart<TimeDilationGrenade>();
		if (part11 != null)
		{
			part11.Range *= 2;
		}
		CheckLiquidVolume(Object);
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCleaveAmountEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != GetIntrinsicWeightEvent.ID && ID != GetIntrinsicValueEvent.ID && ID != PooledEvent<GetMissileWeaponPerformanceEvent>.ID && ID != PooledEvent<GetThrownWeaponPerformanceEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<GetSlotsRequiredEvent>.ID && ID != PooledEvent<GetTonicDosageEvent>.ID && ID != PooledEvent<ModificationAppliedEvent>.ID)
		{
			return ID == WasDerivedFromEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.ApplyToEach((GameObject obj) =>
		{
			obj.RemovePart<ModGigantic>();
		});
		E.ApplyToEach((GameObject obj) =>
		{
			obj.AddPart(new ModGigantic(this));
		});
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Object.HasTagOrProperty("ModGiganticNoDisplayName") && (!E.Object.HasTagOrProperty("ModGiganticNoUnknownDisplayName") || E.Understood()) && !E.Object.HasProperName)
		{
			E.ApplySizeAdjective("gigantic", 30, -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!E.Object.HasTagOrProperty("ModGiganticNoShortDescription"))
		{
			E.Postfix.AppendRules(GetInstanceDescription());
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetIntrinsicWeightEvent E)
	{
		E.Weight += GetAddedWeight();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetIntrinsicValueEvent E)
	{
		if (E.Object.GetIntProperty("Currency") > 0)
		{
			E.Value *= 3.333333;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		if (IsObjectActivePartSubject(E.Subject) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.GetDamageRoll()?.AdjustResult(3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetThrownWeaponPerformanceEvent E)
	{
		if (IsObjectActivePartSubject(E.Object) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.DamageModifier += 3;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetCleaveAmountEvent E)
	{
		if (IsObjectActivePartSubject(E.Object) && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSlotsRequiredEvent E)
	{
		if (IsObjectActivePartSubject(E.Object) && !E.Actor.IsGiganticCreature && !E.Object.IsNatural())
		{
			E.Increases++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTonicDosageEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.Dosage++;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModificationAppliedEvent E)
	{
		CheckLiquidVolume(ParentObject);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GetSlamMultiplier");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetSlamMultiplier")
		{
			E.SetParameter("Multiplier", E.GetIntParameter("Multiplier") + 1);
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Gigantic: This item is much heavier than usual and can only be equipped by gigantic creatures.";
	}

	public static string GetDescription(int Tier, GameObject Object)
	{
		if (Object == null)
		{
			return GetDescription(Tier);
		}
		string text = "item";
		List<List<string>> list = new List<List<string>>();
		List<List<string>> list2 = new List<List<string>>();
		if (Object.LiquidVolume != null)
		{
			list2.Add(new List<string> { "hold", "twice as much liquid" });
		}
		if (Object.HasPart<EnergyCell>())
		{
			list2.Add(new List<string> { "have", "twice the energy capacity" });
		}
		if (Object.HasPartDescendedFrom<IGrenade>())
		{
			list2.Add(new List<string> { "have", "twice as large a radius of effect" });
		}
		if (Object.HasPart<Tonic>())
		{
			list2.Add(new List<string> { "contain", "double the tonic dosage" });
		}
		if (Object.GetIntProperty("Currency") > 0)
		{
			list2.Add(new List<string> { null, "much more valuable" });
		}
		bool flag = Object.EquipAsDefaultBehavior();
		if (!flag)
		{
			list2.Add(new List<string> { null, "much heavier than usual" });
		}
		MeleeWeapon part = Object.GetPart<MeleeWeapon>();
		bool flag2 = flag || Object.IsEntirelyFloating();
		if (part != null && Object.HasTagOrProperty("ShowMeleeWeaponStats"))
		{
			text = "weapon";
			list.Add(new List<string> { "have", "+3 damage" });
			if (part.Skill == "Cudgel")
			{
				list.Add(new List<string>
				{
					null,
					"twice as effective when you Slam with " + Object.them
				});
			}
			else if (part.Skill == "Axe")
			{
				list.Add(new List<string> { "cleave", "for -3 AV" });
			}
			if (!flag2)
			{
				if (Object.UsesSlots == null)
				{
					list2.Add(new List<string>
					{
						"",
						Object.it + " must be wielded " + (Object.UsesTwoSlots ? "four" : "two") + "-handed by non-gigantic creatures"
					});
				}
				else
				{
					list2.Add(new List<string> { "", "can only be equipped by gigantic creatures" });
				}
				flag2 = true;
			}
		}
		else if (Object.HasPart<MissileWeapon>())
		{
			list.Add(new List<string> { "have", "+3 damage" });
			if (!flag2)
			{
				if (Object.UsesSlots == null)
				{
					list2.Add(new List<string>
					{
						"",
						Object.it + " must be wielded " + (Object.UsesTwoSlots ? "four" : "two") + "-handed by non-gigantic creatures"
					});
				}
				else
				{
					list2.Add(new List<string> { "", "can only be equipped by gigantic creatures" });
				}
				flag2 = true;
			}
		}
		else if (Object.HasPart<ThrownWeapon>())
		{
			text = "weapon";
			if (!Object.HasPartDescendedFrom<IGrenade>())
			{
				list.Add(new List<string> { "have", "+3 damage" });
			}
			if (!flag2)
			{
				list2.Add(new List<string> { "", "can only be equipped by gigantic creatures" });
				flag2 = true;
			}
		}
		else if ((Object.HasPart<Armor>() || Object.HasPart<Shield>()) && !flag2)
		{
			list2.Add(new List<string> { "", "can only be equipped by gigantic creatures" });
			flag2 = true;
		}
		if (Object.HasPart<DiggingTool>() || Object.HasPart<Drill>())
		{
			list.Add(new List<string> { "dig", "twice as fast" });
		}
		if (list.Count == 0)
		{
			List<string> list3 = new List<string>();
			foreach (List<string> item in list2)
			{
				list3.Add(GetProcessedItem(item, second: false, list2, Object));
			}
			return "Gigantic: " + (Object.IsPlural ? ("These " + Grammar.Pluralize(text)) : ("This " + text)) + " " + Grammar.MakeAndList(list3) + ".";
		}
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		foreach (List<string> item2 in list)
		{
			list4.Add(GetProcessedItem(item2, second: false, list, Object));
		}
		foreach (List<string> item3 in list2)
		{
			list5.Add(GetProcessedItem(item3, second: true, list2, Object));
		}
		return "Gigantic: " + (Object.IsPlural ? ("These " + Grammar.Pluralize(text)) : ("This " + text)) + " " + Grammar.MakeAndList(list4) + ". " + Grammar.MakeAndList(list5) + ".";
	}

	private static string GetProcessedItem(List<string> item, bool second, List<List<string>> items, GameObject obj)
	{
		if (item[0] == "")
		{
			if (second && item == items[0])
			{
				return obj.It + " " + item[1];
			}
			return item[1];
		}
		if (item[0] == null)
		{
			if (second && item == items[0])
			{
				return obj.Itis + " " + item[1];
			}
			if (item != items[0])
			{
				bool flag = true;
				foreach (List<string> item2 in items)
				{
					if (item2[0] != null)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return item[1];
				}
			}
			return obj.GetVerb("are", PrependSpace: false) + " " + item[1];
		}
		if (second && item == items[0])
		{
			return obj.It + obj.GetVerb(item[0]) + " " + item[1];
		}
		return obj.GetVerb(item[0], PrependSpace: false) + " " + item[1];
	}

	public string GetInstanceDescription()
	{
		return GetDescription(Tier, ParentObject);
	}

	public int GetAddedWeight()
	{
		if (!HaveAddedWeight)
		{
			try
			{
				AddedWeight = ParentObject.GetBlueprint().GetPartParameter("Physics", "Weight", 0) * 4;
			}
			catch
			{
			}
			if (AddedWeight < 4)
			{
				AddedWeight = 4;
			}
			HaveAddedWeight = true;
		}
		return AddedWeight;
	}

	private static void CheckLiquidVolume(GameObject Object)
	{
		if (Object.HasIntProperty("ModGiganticLiquidVolumeDone"))
		{
			return;
		}
		LiquidVolume liquidVolume = Object.LiquidVolume;
		if (liquidVolume != null)
		{
			liquidVolume.MaxVolume *= 2;
			if (!Object.HasContext())
			{
				liquidVolume.Volume *= 2;
			}
			Object.SetIntProperty("ModGiganticLiquidVolumeDone", 1);
		}
	}
}
