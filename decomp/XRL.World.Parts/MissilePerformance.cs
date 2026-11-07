using System;
using System.Collections.Generic;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class MissilePerformance : IActivePart
{
	public int PenetrationModifier;

	public int PenetrationCapModifier;

	public int DamageDieModifier;

	public int DamageModifier;

	public string AddAttributes;

	public string RemoveAttributes;

	public bool? PenetrateCreatures;

	public bool ShowInShortDescription;

	public MissilePerformance()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		MissilePerformance missilePerformance = p as MissilePerformance;
		if (missilePerformance.PenetrationModifier != PenetrationModifier)
		{
			return false;
		}
		if (missilePerformance.PenetrationCapModifier != PenetrationCapModifier)
		{
			return false;
		}
		if (missilePerformance.DamageDieModifier != DamageDieModifier)
		{
			return false;
		}
		if (missilePerformance.DamageModifier != DamageModifier)
		{
			return false;
		}
		if (missilePerformance.AddAttributes != AddAttributes)
		{
			return false;
		}
		if (missilePerformance.RemoveAttributes != RemoveAttributes)
		{
			return false;
		}
		if (missilePerformance.PenetrateCreatures != PenetrateCreatures)
		{
			return false;
		}
		if (missilePerformance.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetMissileWeaponPerformanceEvent>.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return ShowInShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "PenetrationModifier", PenetrationModifier);
		E.AddEntry(this, "PenetrationCapModifier", PenetrationCapModifier);
		E.AddEntry(this, "DamageDieModifier", DamageDieModifier);
		E.AddEntry(this, "DamageModifier", DamageModifier);
		E.AddEntry(this, "AddAttributes", AddAttributes);
		E.AddEntry(this, "RemoveAttributes", RemoveAttributes);
		E.AddEntry(this, "PenetrateCreatures", PenetrateCreatures);
		E.AddEntry(this, "ShowInShortDescription", ShowInShortDescription);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMissileWeaponPerformanceEvent E)
	{
		if (IsObjectActivePartSubject(E.Subject) && IsReady(E.Active, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (PenetrationModifier != 0)
			{
				E.PenetrationBonus += PenetrationModifier;
			}
			if (PenetrationCapModifier != 0)
			{
				E.PenetrationCap += PenetrationCapModifier;
			}
			if (DamageDieModifier != 0)
			{
				E.GetDamageRoll()?.AdjustDieSize(DamageDieModifier);
			}
			if (DamageModifier != 0)
			{
				E.GetDamageRoll()?.AdjustResult(DamageModifier);
			}
			List<string> list = null;
			bool flag = false;
			if (!RemoveAttributes.IsNullOrEmpty() && !E.Attributes.IsNullOrEmpty())
			{
				List<string> list2 = RemoveAttributes.CachedCommaExpansion();
				if (list == null)
				{
					list = new List<string>(E.Attributes.Split(' '));
				}
				int i = 0;
				for (int count = list2.Count; i < count; i++)
				{
					if (list.Contains(list2[i]))
					{
						list.Remove(list2[i]);
						flag = true;
					}
				}
			}
			if (!AddAttributes.IsNullOrEmpty())
			{
				List<string> list3 = AddAttributes.CachedCommaExpansion();
				if (list == null)
				{
					list = (E.Attributes.IsNullOrEmpty() ? new List<string>() : new List<string>(E.Attributes.Split(' ')));
				}
				int j = 0;
				for (int count2 = list3.Count; j < count2; j++)
				{
					if (!list.Contains(list3[j]))
					{
						list.Add(list3[j]);
						flag = true;
					}
				}
			}
			if (flag)
			{
				E.Attributes = string.Join(" ", list.ToArray());
			}
			if (PenetrateCreatures.HasValue)
			{
				E.PenetrateCreatures = PenetrateCreatures.Value;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			bool flag = ChargeUse > 0 || ChargeMinimum > 0;
			if (PenetrationModifier != 0)
			{
				E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("have") + " " + PenetrationModifier.Signed() + " to " + ParentObject.its + " penetration rolls.");
			}
			if (PenetrationCapModifier != 0)
			{
				E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("have") + " " + PenetrationCapModifier.Signed() + " to the maximum of " + ParentObject.its + " penetration rolls.");
			}
			if (DamageDieModifier != 0)
			{
				E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("have") + " " + DamageDieModifier.Signed() + " to the size of " + ParentObject.its + " damage dice.");
			}
			if (DamageModifier != 0)
			{
				E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("have") + " " + DamageModifier.Signed() + " to " + ParentObject.its + " damage rolls.");
			}
			if (!RemoveAttributes.IsNullOrEmpty())
			{
				List<string> list = RemoveAttributes.CachedCommaExpansion();
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					if (list[i] == "Vorpal")
					{
						E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("do") + " not match " + ParentObject.its + " penetration to the armor of " + ParentObject.its + " targets.");
					}
					else
					{
						E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("remove") + " the attribute " + list[i] + " from its projectiles.");
					}
				}
			}
			if (!AddAttributes.IsNullOrEmpty())
			{
				List<string> list2 = AddAttributes.CachedCommaExpansion();
				int j = 0;
				for (int count2 = list2.Count; j < count2; j++)
				{
					if (list2[j] == "Vorpal")
					{
						E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("match") + " " + ParentObject.its + " penetration to the armor of " + ParentObject.its + " targets.");
					}
					else
					{
						E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("add") + " the attribute " + list2[j] + " to its projectiles.");
					}
				}
			}
			if (PenetrateCreatures.HasValue)
			{
				E.Postfix.AppendRules((flag ? ("When powered, " + ParentObject.indicativeProximal) : ParentObject.IndicativeProximal) + " " + (ParentObject.IsPlural ? Grammar.Pluralize(ParentObject.GetDescriptiveCategory()) : ParentObject.GetDescriptiveCategory()) + ParentObject.GetVerb("penetrate") + " creatures.");
			}
		}
		return base.HandleEvent(E);
	}

	public bool WantAddAttribute(string attr)
	{
		if (AddAttributes.IsNullOrEmpty())
		{
			AddAttributes = attr;
			return true;
		}
		List<string> list = AddAttributes.CachedCommaExpansion();
		if (!list.Contains(attr))
		{
			list.Add(attr);
			AddAttributes = string.Join(",", list.ToArray());
			return true;
		}
		return false;
	}

	public bool WantRemoveAttribute(string attr)
	{
		if (RemoveAttributes.IsNullOrEmpty())
		{
			RemoveAttributes = attr;
			return true;
		}
		List<string> list = RemoveAttributes.CachedCommaExpansion();
		if (!list.Contains(attr))
		{
			list.Remove(attr);
			RemoveAttributes = string.Join(",", list.ToArray());
			return true;
		}
		return false;
	}
}
